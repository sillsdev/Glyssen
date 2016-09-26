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
			this.m_tabPageGeneral = new System.Windows.Forms.TabPage();
			this.m_tableLayoutMain = new System.Windows.Forms.TableLayoutPanel();
			this.panel1 = new System.Windows.Forms.Panel();
			this.m_lblAudioStockNumber = new System.Windows.Forms.Label();
			this.m_txtAudioStockNumber = new System.Windows.Forms.TextBox();
			this.m_tabPageWritingSystem = new System.Windows.Forms.TabPage();
			this.m_tabPageTitleAndChapterAnnouncmentOptions = new System.Windows.Forms.TabPage();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.m_tableLayoutPanelAnnouncmentsExample = new System.Windows.Forms.TableLayoutPanel();
			this.m_lblFirstChapterExample = new System.Windows.Forms.Label();
			this.m_lblSubsequentChaptersExample = new System.Windows.Forms.Label();
			this.m_lblSingleChapterBookExample = new System.Windows.Forms.Label();
			this.m_lblChapterAnnouncement = new System.Windows.Forms.Label();
			this.m_lblBookTitleHeading = new System.Windows.Forms.Label();
			this.m_lblExampleTitleForMultipleChapterBook = new System.Windows.Forms.Label();
			this.m_lblExampleFirstChapterAnnouncement = new System.Windows.Forms.Label();
			this.m_lblExampleSubsequentChapterAnnouncement = new System.Windows.Forms.Label();
			this.m_lblExampleTitleForSingleChapterBook = new System.Windows.Forms.Label();
			this.m_lblExampleSingleChapterAnnouncement = new System.Windows.Forms.Label();
			this.m_chkAnnounceChaptersForSingleChapterBooks = new System.Windows.Forms.CheckBox();
			this.m_lblChapterAnnouncementStyle = new System.Windows.Forms.Label();
			this.m_lblBookNameSource = new System.Windows.Forms.Label();
			this.m_rdoBookNamePlusChapterNumber = new System.Windows.Forms.RadioButton();
			this.m_lblExample = new System.Windows.Forms.Label();
			this.m_rdoChapterLabel = new System.Windows.Forms.RadioButton();
			this.m_rdoCustom = new System.Windows.Forms.RadioButton();
			this.m_chkChapterOneAnnouncements = new System.Windows.Forms.CheckBox();
			this.m_cboBookMarker = new System.Windows.Forms.ComboBox();
			this.m_lblChapterAnnouncementWarning = new System.Windows.Forms.Label();
			this.m_tabPageReferenceTexts = new System.Windows.Forms.TabPage();
			this.m_tableLayoutReferenceTexts = new System.Windows.Forms.TableLayoutPanel();
			this.m_labelReferenceText = new System.Windows.Forms.Label();
			this.m_ReferenceText = new System.Windows.Forms.ComboBox();
			this.m_referenceTextExplanation = new System.Windows.Forms.Label();
			this.m_tabPageScriptOptions = new System.Windows.Forms.TabPage();
			this.m_tableLayoutScriptOptions = new System.Windows.Forms.TableLayoutPanel();
			this.m_labelBookIntro = new System.Windows.Forms.Label();
			this.m_bookIntro = new System.Windows.Forms.ComboBox();
			this.m_labelSectionHeadings = new System.Windows.Forms.Label();
			this.m_labelTitleChapter = new System.Windows.Forms.Label();
			this.m_sectionHeadings = new System.Windows.Forms.ComboBox();
			this.m_titleChapters = new System.Windows.Forms.ComboBox();
			this.glyssenColorPalette = new Glyssen.Utilities.GlyssenColorPalette();
			this.m_tabControl = new System.Windows.Forms.TabControl();
			this.m_linkRefTextAttribution = new System.Windows.Forms.LinkLabel();
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).BeginInit();
			this.m_tabPageGeneral.SuspendLayout();
			this.m_tableLayoutMain.SuspendLayout();
			this.panel1.SuspendLayout();
			this.m_tabPageWritingSystem.SuspendLayout();
			this.m_tabPageTitleAndChapterAnnouncmentOptions.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.m_tableLayoutPanelAnnouncmentsExample.SuspendLayout();
			this.m_tabPageReferenceTexts.SuspendLayout();
			this.m_tableLayoutReferenceTexts.SuspendLayout();
			this.m_tabPageScriptOptions.SuspendLayout();
			this.m_tableLayoutScriptOptions.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.glyssenColorPalette)).BeginInit();
			this.m_tabControl.SuspendLayout();
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
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_btnCancel, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.glyssenColorPalette.SetForeColor(this.m_btnCancel, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnCancel, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnCancel, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_btnCancel, L10NSharp.LocalizationPriority.High);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnCancel, "Common.Cancel");
			this.m_btnCancel.Location = new System.Drawing.Point(546, 383);
			this.m_btnCancel.Name = "m_btnCancel";
			this.m_btnCancel.Size = new System.Drawing.Size(75, 23);
			this.m_btnCancel.TabIndex = 2;
			this.m_btnCancel.Text = "Cancel";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_btnCancel, false);
			this.m_btnCancel.UseVisualStyleBackColor = true;
			this.m_btnCancel.Click += new System.EventHandler(this.HandleCancelButtonClick);
			// 
			// m_lblLanguageName
			// 
			this.m_lblLanguageName.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_lblLanguageName.AutoSize = true;
			this.m_lblLanguageName.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblLanguageName, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_lblLanguageName, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblLanguageName.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblLanguageName, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblLanguageName, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblLanguageName, "DialogBoxes.ProjectSettingsDlg.GeneralTab.LanguageName");
			this.m_lblLanguageName.Location = new System.Drawing.Point(6, 111);
			this.m_lblLanguageName.Name = "m_lblLanguageName";
			this.m_lblLanguageName.Size = new System.Drawing.Size(89, 13);
			this.m_lblLanguageName.TabIndex = 0;
			this.m_lblLanguageName.Text = "Language Name:";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblLanguageName, true);
			// 
			// m_lblIso639_2_Code
			// 
			this.m_lblIso639_2_Code.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_lblIso639_2_Code.AutoSize = true;
			this.m_lblIso639_2_Code.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblIso639_2_Code, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_lblIso639_2_Code, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblIso639_2_Code.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblIso639_2_Code, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblIso639_2_Code, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblIso639_2_Code, "DialogBoxes.ProjectSettingsDlg.GeneralTab.EthnologueCode");
			this.m_lblIso639_2_Code.Location = new System.Drawing.Point(6, 124);
			this.m_lblIso639_2_Code.Name = "m_lblIso639_2_Code";
			this.m_lblIso639_2_Code.Size = new System.Drawing.Size(149, 13);
			this.m_lblIso639_2_Code.TabIndex = 2;
			this.m_lblIso639_2_Code.Text = "Ethnologue (ISO 639-2) Code:";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblIso639_2_Code, true);
			// 
			// m_lblPublicationName
			// 
			this.m_lblPublicationName.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_lblPublicationName.AutoSize = true;
			this.m_lblPublicationName.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblPublicationName, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_lblPublicationName, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblPublicationName.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblPublicationName, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblPublicationName, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblPublicationName, "DialogBoxes.ProjectSettingsDlg.GeneralTab.PublicationName");
			this.m_lblPublicationName.Location = new System.Drawing.Point(6, 152);
			this.m_lblPublicationName.Name = "m_lblPublicationName";
			this.m_lblPublicationName.Size = new System.Drawing.Size(93, 13);
			this.m_lblPublicationName.TabIndex = 4;
			this.m_lblPublicationName.Text = "Publication Name:";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblPublicationName, true);
			// 
			// m_lblPublicationId
			// 
			this.m_lblPublicationId.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_lblPublicationId.AutoSize = true;
			this.m_lblPublicationId.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblPublicationId, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_lblPublicationId, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblPublicationId.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblPublicationId, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblPublicationId, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblPublicationId, "DialogBoxes.ProjectSettingsDlg.GeneralTab.PublicationId");
			this.m_lblPublicationId.Location = new System.Drawing.Point(6, 165);
			this.m_lblPublicationId.Name = "m_lblPublicationId";
			this.m_lblPublicationId.Size = new System.Drawing.Size(74, 13);
			this.m_lblPublicationId.TabIndex = 6;
			this.m_lblPublicationId.Text = "Publication Id:";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblPublicationId, true);
			// 
			// m_btnOk
			// 
			this.m_btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.glyssenColorPalette.SetBackColor(this.m_btnOk, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_btnOk, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.glyssenColorPalette.SetForeColor(this.m_btnOk, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnOk, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnOk, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_btnOk, L10NSharp.LocalizationPriority.High);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnOk, "Common.OK");
			this.m_btnOk.Location = new System.Drawing.Point(465, 383);
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
			this.m_wsFontControl.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_wsFontControl, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_wsFontControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_wsFontControl.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
			this.m_wsFontControl.ForeColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetForeColor(this.m_wsFontControl, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_wsFontControl, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_wsFontControl, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_wsFontControl, "DialogBoxes.ProjectSettingsDlg.WritingSystemsTab.WSFontControl");
			this.m_wsFontControl.Location = new System.Drawing.Point(7, 15);
			this.m_wsFontControl.Margin = new System.Windows.Forms.Padding(0);
			this.m_wsFontControl.Name = "m_wsFontControl";
			this.m_wsFontControl.Size = new System.Drawing.Size(589, 310);
			this.m_wsFontControl.TabIndex = 7;
			this.m_wsFontControl.TestAreaText = "";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_wsFontControl, true);
			// 
			// m_lblRecordingProjectName
			// 
			this.m_lblRecordingProjectName.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_lblRecordingProjectName.AutoSize = true;
			this.m_lblRecordingProjectName.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblRecordingProjectName, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_lblRecordingProjectName, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblRecordingProjectName.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblRecordingProjectName, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblRecordingProjectName, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblRecordingProjectName, "DialogBoxes.ProjectSettingsDlg.GeneralTab.RecordingProjectName");
			this.m_lblRecordingProjectName.Location = new System.Drawing.Point(6, 8);
			this.m_lblRecordingProjectName.Name = "m_lblRecordingProjectName";
			this.m_lblRecordingProjectName.Size = new System.Drawing.Size(126, 13);
			this.m_lblRecordingProjectName.TabIndex = 12;
			this.m_lblRecordingProjectName.Text = "Recording Project Name:";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblRecordingProjectName, true);
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
			this.m_l10NSharpExtender.SetLocalizingId(this.m_txtRecordingProjectName, "DialogBoxes.ProjectSettingsDlg.GeneralTab.m_txtProjectName");
			this.m_txtRecordingProjectName.Location = new System.Drawing.Point(161, 3);
			this.m_txtRecordingProjectName.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.m_txtRecordingProjectName.Name = "m_txtRecordingProjectName";
			this.m_txtRecordingProjectName.Size = new System.Drawing.Size(422, 20);
			this.m_txtRecordingProjectName.TabIndex = 0;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_txtRecordingProjectName, false);
			this.m_txtRecordingProjectName.TextChanged += new System.EventHandler(this.m_txtRecordingProjectName_TextChanged);
			// 
			// m_txtLanguageName
			// 
			this.m_txtLanguageName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_txtLanguageName.AutoSize = true;
			this.m_txtLanguageName.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_txtLanguageName, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutMain.SetColumnSpan(this.m_txtLanguageName, 3);
			this.glyssenColorPalette.SetForeColor(this.m_txtLanguageName, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_txtLanguageName.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_txtLanguageName, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_txtLanguageName, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_txtLanguageName, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_txtLanguageName, "DialogBoxes.ProjectSettingsDlg.GeneralTab.m_txtLanguageName");
			this.m_txtLanguageName.Location = new System.Drawing.Point(161, 111);
			this.m_txtLanguageName.Name = "m_txtLanguageName";
			this.m_txtLanguageName.Size = new System.Drawing.Size(422, 13);
			this.m_txtLanguageName.TabIndex = 23;
			this.m_txtLanguageName.Text = "#";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_txtLanguageName, true);
			// 
			// m_txtIso639_2_Code
			// 
			this.m_txtIso639_2_Code.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_txtIso639_2_Code.AutoSize = true;
			this.m_txtIso639_2_Code.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_txtIso639_2_Code, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutMain.SetColumnSpan(this.m_txtIso639_2_Code, 3);
			this.glyssenColorPalette.SetForeColor(this.m_txtIso639_2_Code, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_txtIso639_2_Code.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_txtIso639_2_Code, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_txtIso639_2_Code, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_txtIso639_2_Code, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_txtIso639_2_Code, "DialogBoxes.ProjectSettingsDlg.GeneralTab.m_txtIso639_2_Code");
			this.m_txtIso639_2_Code.Location = new System.Drawing.Point(161, 124);
			this.m_txtIso639_2_Code.Name = "m_txtIso639_2_Code";
			this.m_txtIso639_2_Code.Size = new System.Drawing.Size(422, 13);
			this.m_txtIso639_2_Code.TabIndex = 24;
			this.m_txtIso639_2_Code.Text = "#";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_txtIso639_2_Code, true);
			// 
			// m_txtPublicationName
			// 
			this.m_txtPublicationName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_txtPublicationName.AutoSize = true;
			this.m_txtPublicationName.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_txtPublicationName, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutMain.SetColumnSpan(this.m_txtPublicationName, 3);
			this.glyssenColorPalette.SetForeColor(this.m_txtPublicationName, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_txtPublicationName.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_txtPublicationName, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_txtPublicationName, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_txtPublicationName, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_txtPublicationName, "DialogBoxes.ProjectSettingsDlg.GeneralTab.m_txtPublicationName");
			this.m_txtPublicationName.Location = new System.Drawing.Point(161, 152);
			this.m_txtPublicationName.Name = "m_txtPublicationName";
			this.m_txtPublicationName.Size = new System.Drawing.Size(422, 13);
			this.m_txtPublicationName.TabIndex = 25;
			this.m_txtPublicationName.Text = "#";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_txtPublicationName, true);
			// 
			// m_txtPublicationId
			// 
			this.m_txtPublicationId.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_txtPublicationId.AutoSize = true;
			this.m_txtPublicationId.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_txtPublicationId, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutMain.SetColumnSpan(this.m_txtPublicationId, 3);
			this.glyssenColorPalette.SetForeColor(this.m_txtPublicationId, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_txtPublicationId.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_txtPublicationId, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_txtPublicationId, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_txtPublicationId, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_txtPublicationId, "DialogBoxes.ProjectSettingsDlg.GeneralTab.m_txtPublicationId");
			this.m_txtPublicationId.Location = new System.Drawing.Point(161, 165);
			this.m_txtPublicationId.Name = "m_txtPublicationId";
			this.m_txtPublicationId.Size = new System.Drawing.Size(422, 13);
			this.m_txtPublicationId.TabIndex = 26;
			this.m_txtPublicationId.Text = "#";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_txtPublicationId, true);
			// 
			// m_lblOriginalBundlePath
			// 
			this.m_lblOriginalBundlePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.m_lblOriginalBundlePath.AutoSize = true;
			this.m_lblOriginalBundlePath.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblOriginalBundlePath, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_lblOriginalBundlePath, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblOriginalBundlePath.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblOriginalBundlePath, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblOriginalBundlePath, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblOriginalBundlePath, "DialogBoxes.ProjectSettingsDlg.GeneralTab.OriginalBundlePath");
			this.m_lblOriginalBundlePath.Location = new System.Drawing.Point(6, 67);
			this.m_lblOriginalBundlePath.Name = "m_lblOriginalBundlePath";
			this.m_lblOriginalBundlePath.Size = new System.Drawing.Size(106, 29);
			this.m_lblOriginalBundlePath.TabIndex = 28;
			this.m_lblOriginalBundlePath.Text = "Original Bundle Path:";
			this.m_lblOriginalBundlePath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblOriginalBundlePath, true);
			// 
			// m_txtOriginalBundlePath
			// 
			this.m_txtOriginalBundlePath.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_txtOriginalBundlePath.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_txtOriginalBundlePath, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutMain.SetColumnSpan(this.m_txtOriginalBundlePath, 2);
			this.glyssenColorPalette.SetForeColor(this.m_txtOriginalBundlePath, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_txtOriginalBundlePath.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_txtOriginalBundlePath, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_txtOriginalBundlePath, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_txtOriginalBundlePath, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_txtOriginalBundlePath, "DialogBoxes.ProjectSettingsDlg.GeneralTab.m_txtOriginalBundlePath");
			this.m_txtOriginalBundlePath.Location = new System.Drawing.Point(161, 67);
			this.m_txtOriginalBundlePath.Name = "m_txtOriginalBundlePath";
			this.m_txtOriginalBundlePath.Size = new System.Drawing.Size(334, 29);
			this.m_txtOriginalBundlePath.TabIndex = 29;
			this.m_txtOriginalBundlePath.Text = "#";
			this.m_txtOriginalBundlePath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_txtOriginalBundlePath, true);
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.glyssenColorPalette.SetBackColor(this.label2, Glyssen.Utilities.GlyssenColors.BackColor);
			this.label2.BackColor = System.Drawing.SystemColors.Control;
			this.label2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.label2.ForeColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetForeColor(this.label2, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.label2, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.label2, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.label2, "DialogBoxes.ProjectSettingsDlg.label1");
			this.label2.Location = new System.Drawing.Point(8, 32);
			this.label2.Margin = new System.Windows.Forms.Padding(3, 0, 3, 10);
			this.label2.Name = "label2";
			this.label2.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
			this.label2.Size = new System.Drawing.Size(561, 2);
			this.label2.TabIndex = 16;
			this.glyssenColorPalette.SetUsePaletteColors(this.label2, true);
			// 
			// m_lblQuoteMarks
			// 
			this.m_lblQuoteMarks.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.m_lblQuoteMarks.AutoSize = true;
			this.glyssenColorPalette.SetBackColor(this.m_lblQuoteMarks, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_lblQuoteMarks.BackColor = System.Drawing.SystemColors.Control;
			this.m_lblQuoteMarks.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_lblQuoteMarks.ForeColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetForeColor(this.m_lblQuoteMarks, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblQuoteMarks, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblQuoteMarks, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblQuoteMarks, "DialogBoxes.ProjectSettingsDlg.GeneralTab.QuoteMarks");
			this.m_lblQuoteMarks.Location = new System.Drawing.Point(0, 22);
			this.m_lblQuoteMarks.Name = "m_lblQuoteMarks";
			this.m_lblQuoteMarks.Size = new System.Drawing.Size(88, 15);
			this.m_lblQuoteMarks.TabIndex = 15;
			this.m_lblQuoteMarks.Text = "Quote Marks";
			this.m_lblQuoteMarks.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblQuoteMarks, true);
			// 
			// m_lblQuoteMarkReview
			// 
			this.m_lblQuoteMarkReview.AutoSize = true;
			this.m_lblQuoteMarkReview.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblQuoteMarkReview, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutMain.SetColumnSpan(this.m_lblQuoteMarkReview, 4);
			this.glyssenColorPalette.SetForeColor(this.m_lblQuoteMarkReview, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblQuoteMarkReview.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblQuoteMarkReview, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblQuoteMarkReview, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_lblQuoteMarkReview, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblQuoteMarkReview, "DialogBoxes.ProjectSettingsDlg.GeneralTab.QuoteMarkReview");
			this.m_lblQuoteMarkReview.Location = new System.Drawing.Point(6, 256);
			this.m_lblQuoteMarkReview.Name = "m_lblQuoteMarkReview";
			this.m_lblQuoteMarkReview.Padding = new System.Windows.Forms.Padding(0, 0, 0, 5);
			this.m_lblQuoteMarkReview.Size = new System.Drawing.Size(67, 18);
			this.m_lblQuoteMarkReview.TabIndex = 20;
			this.m_lblQuoteMarkReview.Text = "Review Text";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblQuoteMarkReview, true);
			// 
			// m_lblQuoteMarkSummary
			// 
			this.m_lblQuoteMarkSummary.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_lblQuoteMarkSummary.AutoSize = true;
			this.m_lblQuoteMarkSummary.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblQuoteMarkSummary, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_lblQuoteMarkSummary.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F);
			this.glyssenColorPalette.SetForeColor(this.m_lblQuoteMarkSummary, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblQuoteMarkSummary.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblQuoteMarkSummary, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblQuoteMarkSummary, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_lblQuoteMarkSummary, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblQuoteMarkSummary, "DialogBoxes.ProjectSettingsDlg.GeneralTab.QuoteMarkSummary");
			this.m_lblQuoteMarkSummary.Location = new System.Drawing.Point(161, 274);
			this.m_lblQuoteMarkSummary.Name = "m_lblQuoteMarkSummary";
			this.m_lblQuoteMarkSummary.Size = new System.Drawing.Size(132, 36);
			this.m_lblQuoteMarkSummary.TabIndex = 22;
			this.m_lblQuoteMarkSummary.Text = "Summary Text";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblQuoteMarkSummary, true);
			// 
			// m_btnQuoteMarkSettings
			// 
			this.m_btnQuoteMarkSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.m_btnQuoteMarkSettings.AutoSize = true;
			this.m_btnQuoteMarkSettings.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.glyssenColorPalette.SetBackColor(this.m_btnQuoteMarkSettings, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutMain.SetColumnSpan(this.m_btnQuoteMarkSettings, 2);
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_btnQuoteMarkSettings, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.glyssenColorPalette.SetForeColor(this.m_btnQuoteMarkSettings, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnQuoteMarkSettings, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnQuoteMarkSettings, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnQuoteMarkSettings, "DialogBoxes.ProjectSettingsDlg.GeneralTab.ReviewChangeQuoteMarkSettings");
			this.m_btnQuoteMarkSettings.Location = new System.Drawing.Point(460, 277);
			this.m_btnQuoteMarkSettings.Name = "m_btnQuoteMarkSettings";
			this.m_btnQuoteMarkSettings.Size = new System.Drawing.Size(123, 23);
			this.m_btnQuoteMarkSettings.TabIndex = 3;
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
			this.m_txtVersification.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_txtVersification, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutMain.SetColumnSpan(this.m_txtVersification, 3);
			this.glyssenColorPalette.SetForeColor(this.m_txtVersification, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_txtVersification.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_txtVersification, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_txtVersification, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_txtVersification, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_txtVersification, "DialogBoxes.ProjectSettingsDlg.GeneralTab.m_txtVersification");
			this.m_txtVersification.Location = new System.Drawing.Point(161, 193);
			this.m_txtVersification.Name = "m_txtVersification";
			this.m_txtVersification.Size = new System.Drawing.Size(422, 13);
			this.m_txtVersification.TabIndex = 27;
			this.m_txtVersification.Text = "#";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_txtVersification, true);
			// 
			// m_lblVersification
			// 
			this.m_lblVersification.AutoSize = true;
			this.m_lblVersification.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblVersification, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_lblVersification, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblVersification.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblVersification, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblVersification, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblVersification, "DialogBoxes.ProjectSettingsDlg.GeneralTab.Versification");
			this.m_lblVersification.Location = new System.Drawing.Point(6, 193);
			this.m_lblVersification.Name = "m_lblVersification";
			this.m_lblVersification.Size = new System.Drawing.Size(67, 13);
			this.m_lblVersification.TabIndex = 17;
			this.m_lblVersification.Text = "Versification:";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblVersification, true);
			// 
			// m_lblSummary
			// 
			this.m_lblSummary.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.m_lblSummary.AutoSize = true;
			this.m_lblSummary.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblSummary, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_lblSummary, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblSummary.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblSummary, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblSummary, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblSummary, "DialogBoxes.ProjectSettingsDlg.GeneralTab.Summary");
			this.m_lblSummary.Location = new System.Drawing.Point(6, 274);
			this.m_lblSummary.Name = "m_lblSummary";
			this.m_lblSummary.Size = new System.Drawing.Size(53, 36);
			this.m_lblSummary.TabIndex = 23;
			this.m_lblSummary.Text = "Summary:";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblSummary, true);
			// 
			// m_btnUpdateFromBundle
			// 
			this.m_btnUpdateFromBundle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.m_btnUpdateFromBundle.AutoSize = true;
			this.glyssenColorPalette.SetBackColor(this.m_btnUpdateFromBundle, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_btnUpdateFromBundle, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.glyssenColorPalette.SetForeColor(this.m_btnUpdateFromBundle, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnUpdateFromBundle, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnUpdateFromBundle, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnUpdateFromBundle, "DialogBoxes.ProjectSettingsDlg.GeneralTab.Update");
			this.m_btnUpdateFromBundle.Location = new System.Drawing.Point(508, 70);
			this.m_btnUpdateFromBundle.Margin = new System.Windows.Forms.Padding(10, 3, 3, 3);
			this.m_btnUpdateFromBundle.Name = "m_btnUpdateFromBundle";
			this.m_btnUpdateFromBundle.Size = new System.Drawing.Size(75, 23);
			this.m_btnUpdateFromBundle.TabIndex = 2;
			this.m_btnUpdateFromBundle.Text = "Update...";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_btnUpdateFromBundle, false);
			this.m_btnUpdateFromBundle.UseVisualStyleBackColor = true;
			this.m_btnUpdateFromBundle.Click += new System.EventHandler(this.m_btnUpdateFromBundle_Click);
			// 
			// m_tabPageGeneral
			// 
			this.glyssenColorPalette.SetBackColor(this.m_tabPageGeneral, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tabPageGeneral.BackColor = System.Drawing.SystemColors.Control;
			this.m_tabPageGeneral.Controls.Add(this.m_tableLayoutMain);
			this.glyssenColorPalette.SetForeColor(this.m_tabPageGeneral, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_tabPageGeneral.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_tabPageGeneral, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_tabPageGeneral, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_tabPageGeneral, "DialogBoxes.ProjectSettingsDlg.GeneralTab.TabName");
			this.m_tabPageGeneral.Location = new System.Drawing.Point(4, 22);
			this.m_tabPageGeneral.Name = "m_tabPageGeneral";
			this.m_tabPageGeneral.Padding = new System.Windows.Forms.Padding(7, 12, 7, 12);
			this.m_tabPageGeneral.Size = new System.Drawing.Size(603, 337);
			this.m_tabPageGeneral.TabIndex = 0;
			this.m_tabPageGeneral.Text = "General";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_tabPageGeneral, true);
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
			this.m_tableLayoutMain.Controls.Add(this.m_lblQuoteMarkSummary, 1, 15);
			this.m_tableLayoutMain.Controls.Add(this.m_lblQuoteMarkReview, 0, 14);
			this.m_tableLayoutMain.Controls.Add(this.m_lblSummary, 0, 15);
			this.m_tableLayoutMain.Controls.Add(this.m_lblRecordingProjectName, 0, 0);
			this.m_tableLayoutMain.Controls.Add(this.m_lblLanguageName, 0, 5);
			this.m_tableLayoutMain.Controls.Add(this.m_lblPublicationId, 0, 9);
			this.m_tableLayoutMain.Controls.Add(this.m_lblIso639_2_Code, 0, 6);
			this.m_tableLayoutMain.Controls.Add(this.m_lblPublicationName, 0, 8);
			this.m_tableLayoutMain.Controls.Add(this.panel1, 0, 13);
			this.m_tableLayoutMain.Controls.Add(this.m_txtLanguageName, 1, 5);
			this.m_tableLayoutMain.Controls.Add(this.m_txtIso639_2_Code, 1, 6);
			this.m_tableLayoutMain.Controls.Add(this.m_txtPublicationName, 1, 8);
			this.m_tableLayoutMain.Controls.Add(this.m_txtPublicationId, 1, 9);
			this.m_tableLayoutMain.Controls.Add(this.m_lblOriginalBundlePath, 0, 3);
			this.m_tableLayoutMain.Controls.Add(this.m_txtOriginalBundlePath, 1, 3);
			this.m_tableLayoutMain.Controls.Add(this.m_lblVersification, 0, 12);
			this.m_tableLayoutMain.Controls.Add(this.m_txtVersification, 1, 12);
			this.m_tableLayoutMain.Controls.Add(this.m_btnUpdateFromBundle, 3, 3);
			this.m_tableLayoutMain.Controls.Add(this.m_btnQuoteMarkSettings, 2, 15);
			this.m_tableLayoutMain.Controls.Add(this.m_lblAudioStockNumber, 0, 1);
			this.m_tableLayoutMain.Controls.Add(this.m_txtAudioStockNumber, 1, 1);
			this.m_tableLayoutMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.glyssenColorPalette.SetForeColor(this.m_tableLayoutMain, Glyssen.Utilities.GlyssenColors.Default);
			this.m_tableLayoutMain.Location = new System.Drawing.Point(7, 12);
			this.m_tableLayoutMain.Margin = new System.Windows.Forms.Padding(0);
			this.m_tableLayoutMain.Name = "m_tableLayoutMain";
			this.m_tableLayoutMain.Padding = new System.Windows.Forms.Padding(3);
			this.m_tableLayoutMain.RowCount = 16;
			this.m_tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
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
			this.m_tableLayoutMain.Size = new System.Drawing.Size(589, 313);
			this.m_tableLayoutMain.TabIndex = 0;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_tableLayoutMain, false);
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
			this.panel1.Location = new System.Drawing.Point(6, 209);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(577, 44);
			this.panel1.TabIndex = 19;
			this.glyssenColorPalette.SetUsePaletteColors(this.panel1, false);
			// 
			// m_lblAudioStockNumber
			// 
			this.m_lblAudioStockNumber.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_lblAudioStockNumber.AutoSize = true;
			this.m_lblAudioStockNumber.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblAudioStockNumber, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_lblAudioStockNumber, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblAudioStockNumber.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblAudioStockNumber, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblAudioStockNumber, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblAudioStockNumber, "DialogBoxes.ProjectSettingsDlg.GeneralTab.AudiStockNumber");
			this.m_lblAudioStockNumber.Location = new System.Drawing.Point(6, 32);
			this.m_lblAudioStockNumber.Name = "m_lblAudioStockNumber";
			this.m_lblAudioStockNumber.Size = new System.Drawing.Size(108, 13);
			this.m_lblAudioStockNumber.TabIndex = 31;
			this.m_lblAudioStockNumber.Text = "Audio Stock Number:";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblAudioStockNumber, true);
			// 
			// m_txtAudioStockNumber
			// 
			this.m_txtAudioStockNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.glyssenColorPalette.SetBackColor(this.m_txtAudioStockNumber, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutMain.SetColumnSpan(this.m_txtAudioStockNumber, 3);
			this.glyssenColorPalette.SetForeColor(this.m_txtAudioStockNumber, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_txtAudioStockNumber, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_txtAudioStockNumber, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_txtAudioStockNumber, "DialogBoxes.ProjectSettingsDlg.textBox1");
			this.m_txtAudioStockNumber.Location = new System.Drawing.Point(161, 29);
			this.m_txtAudioStockNumber.Name = "m_txtAudioStockNumber";
			this.m_txtAudioStockNumber.Size = new System.Drawing.Size(422, 20);
			this.m_txtAudioStockNumber.TabIndex = 1;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_txtAudioStockNumber, false);
			// 
			// m_tabPageWritingSystem
			// 
			this.glyssenColorPalette.SetBackColor(this.m_tabPageWritingSystem, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tabPageWritingSystem.BackColor = System.Drawing.SystemColors.Control;
			this.m_tabPageWritingSystem.Controls.Add(this.m_wsFontControl);
			this.glyssenColorPalette.SetForeColor(this.m_tabPageWritingSystem, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_tabPageWritingSystem.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_tabPageWritingSystem, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_tabPageWritingSystem, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_tabPageWritingSystem, "DialogBoxes.ProjectSettingsDlg.WritingSystemsTab.TabName");
			this.m_tabPageWritingSystem.Location = new System.Drawing.Point(4, 22);
			this.m_tabPageWritingSystem.Name = "m_tabPageWritingSystem";
			this.m_tabPageWritingSystem.Padding = new System.Windows.Forms.Padding(7, 15, 7, 12);
			this.m_tabPageWritingSystem.Size = new System.Drawing.Size(603, 337);
			this.m_tabPageWritingSystem.TabIndex = 1;
			this.m_tabPageWritingSystem.Text = "Writing System";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_tabPageWritingSystem, true);
			// 
			// m_tabPageTitleAndChapterAnnouncmentOptions
			// 
			this.glyssenColorPalette.SetBackColor(this.m_tabPageTitleAndChapterAnnouncmentOptions, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tabPageTitleAndChapterAnnouncmentOptions.BackColor = System.Drawing.SystemColors.Control;
			this.m_tabPageTitleAndChapterAnnouncmentOptions.Controls.Add(this.tableLayoutPanel1);
			this.glyssenColorPalette.SetForeColor(this.m_tabPageTitleAndChapterAnnouncmentOptions, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_tabPageTitleAndChapterAnnouncmentOptions.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_tabPageTitleAndChapterAnnouncmentOptions, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_tabPageTitleAndChapterAnnouncmentOptions, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_tabPageTitleAndChapterAnnouncmentOptions, "DialogBoxes.ProjectSettingsDlg.ChapterAnnouncementTab.TabName");
			this.m_tabPageTitleAndChapterAnnouncmentOptions.Location = new System.Drawing.Point(4, 22);
			this.m_tabPageTitleAndChapterAnnouncmentOptions.Name = "m_tabPageTitleAndChapterAnnouncmentOptions";
			this.m_tabPageTitleAndChapterAnnouncmentOptions.Padding = new System.Windows.Forms.Padding(10, 12, 7, 12);
			this.m_tabPageTitleAndChapterAnnouncmentOptions.Size = new System.Drawing.Size(603, 337);
			this.m_tabPageTitleAndChapterAnnouncmentOptions.TabIndex = 2;
			this.m_tabPageTitleAndChapterAnnouncmentOptions.Text = "Chapter Announcements";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_tabPageTitleAndChapterAnnouncmentOptions, true);
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.tableLayoutPanel1, Glyssen.Utilities.GlyssenColors.BackColor);
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.m_tableLayoutPanelAnnouncmentsExample, 0, 7);
			this.tableLayoutPanel1.Controls.Add(this.m_chkAnnounceChaptersForSingleChapterBooks, 0, 5);
			this.tableLayoutPanel1.Controls.Add(this.m_lblChapterAnnouncementStyle, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.m_lblBookNameSource, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.m_rdoBookNamePlusChapterNumber, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.m_lblExample, 0, 6);
			this.tableLayoutPanel1.Controls.Add(this.m_rdoChapterLabel, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.m_rdoCustom, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.m_chkChapterOneAnnouncements, 0, 4);
			this.tableLayoutPanel1.Controls.Add(this.m_cboBookMarker, 2, 1);
			this.tableLayoutPanel1.Controls.Add(this.m_lblChapterAnnouncementWarning, 2, 2);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.glyssenColorPalette.SetForeColor(this.tableLayoutPanel1, Glyssen.Utilities.GlyssenColors.Default);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(10, 12);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 8;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(586, 313);
			this.tableLayoutPanel1.TabIndex = 3;
			this.glyssenColorPalette.SetUsePaletteColors(this.tableLayoutPanel1, true);
			// 
			// m_tableLayoutPanelAnnouncmentsExample
			// 
			this.m_tableLayoutPanelAnnouncmentsExample.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_tableLayoutPanelAnnouncmentsExample.AutoSize = true;
			this.m_tableLayoutPanelAnnouncmentsExample.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.m_tableLayoutPanelAnnouncmentsExample.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_tableLayoutPanelAnnouncmentsExample, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutPanelAnnouncmentsExample.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
			this.m_tableLayoutPanelAnnouncmentsExample.ColumnCount = 4;
			this.tableLayoutPanel1.SetColumnSpan(this.m_tableLayoutPanelAnnouncmentsExample, 3);
			this.m_tableLayoutPanelAnnouncmentsExample.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.m_tableLayoutPanelAnnouncmentsExample.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.m_tableLayoutPanelAnnouncmentsExample.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.m_tableLayoutPanelAnnouncmentsExample.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.m_tableLayoutPanelAnnouncmentsExample.Controls.Add(this.m_lblFirstChapterExample, 1, 0);
			this.m_tableLayoutPanelAnnouncmentsExample.Controls.Add(this.m_lblSubsequentChaptersExample, 2, 0);
			this.m_tableLayoutPanelAnnouncmentsExample.Controls.Add(this.m_lblSingleChapterBookExample, 3, 0);
			this.m_tableLayoutPanelAnnouncmentsExample.Controls.Add(this.m_lblChapterAnnouncement, 0, 2);
			this.m_tableLayoutPanelAnnouncmentsExample.Controls.Add(this.m_lblBookTitleHeading, 0, 1);
			this.m_tableLayoutPanelAnnouncmentsExample.Controls.Add(this.m_lblExampleTitleForMultipleChapterBook, 1, 1);
			this.m_tableLayoutPanelAnnouncmentsExample.Controls.Add(this.m_lblExampleFirstChapterAnnouncement, 1, 2);
			this.m_tableLayoutPanelAnnouncmentsExample.Controls.Add(this.m_lblExampleSubsequentChapterAnnouncement, 2, 2);
			this.m_tableLayoutPanelAnnouncmentsExample.Controls.Add(this.m_lblExampleTitleForSingleChapterBook, 3, 1);
			this.m_tableLayoutPanelAnnouncmentsExample.Controls.Add(this.m_lblExampleSingleChapterAnnouncement, 3, 2);
			this.glyssenColorPalette.SetForeColor(this.m_tableLayoutPanelAnnouncmentsExample, Glyssen.Utilities.GlyssenColors.Default);
			this.m_tableLayoutPanelAnnouncmentsExample.Location = new System.Drawing.Point(3, 169);
			this.m_tableLayoutPanelAnnouncmentsExample.Name = "m_tableLayoutPanelAnnouncmentsExample";
			this.m_tableLayoutPanelAnnouncmentsExample.RowCount = 3;
			this.m_tableLayoutPanelAnnouncmentsExample.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelAnnouncmentsExample.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelAnnouncmentsExample.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelAnnouncmentsExample.Size = new System.Drawing.Size(580, 64);
			this.m_tableLayoutPanelAnnouncmentsExample.TabIndex = 7;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_tableLayoutPanelAnnouncmentsExample, true);
			// 
			// m_lblFirstChapterExample
			// 
			this.m_lblFirstChapterExample.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.m_lblFirstChapterExample.AutoSize = true;
			this.m_lblFirstChapterExample.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblFirstChapterExample, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_lblFirstChapterExample.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.glyssenColorPalette.SetForeColor(this.m_lblFirstChapterExample, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblFirstChapterExample.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblFirstChapterExample, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblFirstChapterExample, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblFirstChapterExample, "DialogBoxes.ProjectSettingsDlg.ChapterAnnouncementTab.FirstChapterExample");
			this.m_lblFirstChapterExample.Location = new System.Drawing.Point(175, 9);
			this.m_lblFirstChapterExample.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
			this.m_lblFirstChapterExample.Name = "m_lblFirstChapterExample";
			this.m_lblFirstChapterExample.Size = new System.Drawing.Size(66, 13);
			this.m_lblFirstChapterExample.TabIndex = 2;
			this.m_lblFirstChapterExample.Text = "First Chapter";
			this.m_lblFirstChapterExample.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblFirstChapterExample, true);
			// 
			// m_lblSubsequentChaptersExample
			// 
			this.m_lblSubsequentChaptersExample.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.m_lblSubsequentChaptersExample.AutoSize = true;
			this.m_lblSubsequentChaptersExample.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblSubsequentChaptersExample, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_lblSubsequentChaptersExample.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.glyssenColorPalette.SetForeColor(this.m_lblSubsequentChaptersExample, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblSubsequentChaptersExample.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblSubsequentChaptersExample, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblSubsequentChaptersExample, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblSubsequentChaptersExample, "DialogBoxes.ProjectSettingsDlg.ChapterAnnouncementTab.SubsequentChaptersExample");
			this.m_lblSubsequentChaptersExample.Location = new System.Drawing.Point(297, 9);
			this.m_lblSubsequentChaptersExample.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
			this.m_lblSubsequentChaptersExample.Name = "m_lblSubsequentChaptersExample";
			this.m_lblSubsequentChaptersExample.Size = new System.Drawing.Size(109, 13);
			this.m_lblSubsequentChaptersExample.TabIndex = 5;
			this.m_lblSubsequentChaptersExample.Text = "Subsequent Chapters";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblSubsequentChaptersExample, true);
			// 
			// m_lblSingleChapterBookExample
			// 
			this.m_lblSingleChapterBookExample.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.m_lblSingleChapterBookExample.AutoSize = true;
			this.m_lblSingleChapterBookExample.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblSingleChapterBookExample, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_lblSingleChapterBookExample.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.glyssenColorPalette.SetForeColor(this.m_lblSingleChapterBookExample, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblSingleChapterBookExample.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblSingleChapterBookExample, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblSingleChapterBookExample, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblSingleChapterBookExample, "DialogBoxes.ProjectSettingsDlg.ChapterAnnouncementTab.SingleChapterBookExample");
			this.m_lblSingleChapterBookExample.Location = new System.Drawing.Point(443, 9);
			this.m_lblSingleChapterBookExample.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
			this.m_lblSingleChapterBookExample.Name = "m_lblSingleChapterBookExample";
			this.m_lblSingleChapterBookExample.Size = new System.Drawing.Size(103, 13);
			this.m_lblSingleChapterBookExample.TabIndex = 7;
			this.m_lblSingleChapterBookExample.Text = "Single-chapter Book";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblSingleChapterBookExample, true);
			// 
			// m_lblChapterAnnouncement
			// 
			this.m_lblChapterAnnouncement.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_lblChapterAnnouncement.AutoSize = true;
			this.m_lblChapterAnnouncement.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblChapterAnnouncement, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_lblChapterAnnouncement.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.glyssenColorPalette.SetForeColor(this.m_lblChapterAnnouncement, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblChapterAnnouncement.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblChapterAnnouncement, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblChapterAnnouncement, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblChapterAnnouncement, "DialogBoxes.ProjectSettingsDlg.ChapterAnnouncementTab.ChapterAnnouncementHeading");
			this.m_lblChapterAnnouncement.Location = new System.Drawing.Point(4, 43);
			this.m_lblChapterAnnouncement.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.m_lblChapterAnnouncement.Name = "m_lblChapterAnnouncement";
			this.m_lblChapterAnnouncement.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
			this.m_lblChapterAnnouncement.Size = new System.Drawing.Size(119, 19);
			this.m_lblChapterAnnouncement.TabIndex = 1;
			this.m_lblChapterAnnouncement.Text = "Chapter Announcement";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblChapterAnnouncement, true);
			// 
			// m_lblBookTitleHeading
			// 
			this.m_lblBookTitleHeading.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_lblBookTitleHeading.AutoSize = true;
			this.m_lblBookTitleHeading.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblBookTitleHeading, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_lblBookTitleHeading.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.glyssenColorPalette.SetForeColor(this.m_lblBookTitleHeading, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblBookTitleHeading.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblBookTitleHeading, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblBookTitleHeading, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblBookTitleHeading, "DialogBoxes.ProjectSettingsDlg.ChapterAnnouncementTab.BookTitleHeading");
			this.m_lblBookTitleHeading.Location = new System.Drawing.Point(4, 23);
			this.m_lblBookTitleHeading.Name = "m_lblBookTitleHeading";
			this.m_lblBookTitleHeading.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
			this.m_lblBookTitleHeading.Size = new System.Drawing.Size(116, 19);
			this.m_lblBookTitleHeading.TabIndex = 0;
			this.m_lblBookTitleHeading.Text = "Book Title";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblBookTitleHeading, true);
			// 
			// m_lblExampleTitleForMultipleChapterBook
			// 
			this.m_lblExampleTitleForMultipleChapterBook.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_lblExampleTitleForMultipleChapterBook.AutoSize = true;
			this.m_lblExampleTitleForMultipleChapterBook.BackColor = System.Drawing.SystemColors.Window;
			this.glyssenColorPalette.SetBackColor(this.m_lblExampleTitleForMultipleChapterBook, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_lblExampleTitleForMultipleChapterBook, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblExampleTitleForMultipleChapterBook, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblExampleTitleForMultipleChapterBook, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_lblExampleTitleForMultipleChapterBook, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblExampleTitleForMultipleChapterBook, "DialogBoxes.ProjectSettingsDlg.ChapterAnnouncementTab.ExampleTitleForMultipleChap" +
        "terBook");
			this.m_lblExampleTitleForMultipleChapterBook.Location = new System.Drawing.Point(124, 23);
			this.m_lblExampleTitleForMultipleChapterBook.Margin = new System.Windows.Forms.Padding(0);
			this.m_lblExampleTitleForMultipleChapterBook.Name = "m_lblExampleTitleForMultipleChapterBook";
			this.m_lblExampleTitleForMultipleChapterBook.Padding = new System.Windows.Forms.Padding(3, 6, 3, 0);
			this.m_lblExampleTitleForMultipleChapterBook.Size = new System.Drawing.Size(169, 19);
			this.m_lblExampleTitleForMultipleChapterBook.TabIndex = 3;
			this.m_lblExampleTitleForMultipleChapterBook.Text = "#";
			this.m_lblExampleTitleForMultipleChapterBook.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblExampleTitleForMultipleChapterBook, false);
			// 
			// m_lblExampleFirstChapterAnnouncement
			// 
			this.m_lblExampleFirstChapterAnnouncement.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_lblExampleFirstChapterAnnouncement.AutoSize = true;
			this.m_lblExampleFirstChapterAnnouncement.BackColor = System.Drawing.SystemColors.Window;
			this.glyssenColorPalette.SetBackColor(this.m_lblExampleFirstChapterAnnouncement, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_lblExampleFirstChapterAnnouncement, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblExampleFirstChapterAnnouncement, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblExampleFirstChapterAnnouncement, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_lblExampleFirstChapterAnnouncement, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblExampleFirstChapterAnnouncement, "DialogBoxes.ProjectSettingsDlg.ChapterAnnouncementTab.ExampleFirstChapterAnnounce" +
        "ment");
			this.m_lblExampleFirstChapterAnnouncement.Location = new System.Drawing.Point(124, 43);
			this.m_lblExampleFirstChapterAnnouncement.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
			this.m_lblExampleFirstChapterAnnouncement.Name = "m_lblExampleFirstChapterAnnouncement";
			this.m_lblExampleFirstChapterAnnouncement.Padding = new System.Windows.Forms.Padding(3, 6, 3, 0);
			this.m_lblExampleFirstChapterAnnouncement.Size = new System.Drawing.Size(169, 19);
			this.m_lblExampleFirstChapterAnnouncement.TabIndex = 4;
			this.m_lblExampleFirstChapterAnnouncement.Text = "#";
			this.m_lblExampleFirstChapterAnnouncement.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblExampleFirstChapterAnnouncement, false);
			// 
			// m_lblExampleSubsequentChapterAnnouncement
			// 
			this.m_lblExampleSubsequentChapterAnnouncement.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_lblExampleSubsequentChapterAnnouncement.AutoSize = true;
			this.m_lblExampleSubsequentChapterAnnouncement.BackColor = System.Drawing.SystemColors.Window;
			this.glyssenColorPalette.SetBackColor(this.m_lblExampleSubsequentChapterAnnouncement, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_lblExampleSubsequentChapterAnnouncement, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblExampleSubsequentChapterAnnouncement, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblExampleSubsequentChapterAnnouncement, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_lblExampleSubsequentChapterAnnouncement, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblExampleSubsequentChapterAnnouncement, "DialogBoxes.ProjectSettingsDlg.ChapterAnnouncementTab.ExampleSubsequentChapterAnn" +
        "ouncement");
			this.m_lblExampleSubsequentChapterAnnouncement.Location = new System.Drawing.Point(294, 43);
			this.m_lblExampleSubsequentChapterAnnouncement.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
			this.m_lblExampleSubsequentChapterAnnouncement.Name = "m_lblExampleSubsequentChapterAnnouncement";
			this.m_lblExampleSubsequentChapterAnnouncement.Padding = new System.Windows.Forms.Padding(3, 6, 3, 0);
			this.m_lblExampleSubsequentChapterAnnouncement.Size = new System.Drawing.Size(115, 19);
			this.m_lblExampleSubsequentChapterAnnouncement.TabIndex = 6;
			this.m_lblExampleSubsequentChapterAnnouncement.Text = "#";
			this.m_lblExampleSubsequentChapterAnnouncement.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblExampleSubsequentChapterAnnouncement, false);
			// 
			// m_lblExampleTitleForSingleChapterBook
			// 
			this.m_lblExampleTitleForSingleChapterBook.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_lblExampleTitleForSingleChapterBook.AutoEllipsis = true;
			this.m_lblExampleTitleForSingleChapterBook.AutoSize = true;
			this.m_lblExampleTitleForSingleChapterBook.BackColor = System.Drawing.SystemColors.Window;
			this.glyssenColorPalette.SetBackColor(this.m_lblExampleTitleForSingleChapterBook, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_lblExampleTitleForSingleChapterBook, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblExampleTitleForSingleChapterBook, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblExampleTitleForSingleChapterBook, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_lblExampleTitleForSingleChapterBook, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblExampleTitleForSingleChapterBook, "DialogBoxes.ProjectSettingsDlg.ChapterAnnouncementTab.ExampleTitleForSingleChapte" +
        "rBook");
			this.m_lblExampleTitleForSingleChapterBook.Location = new System.Drawing.Point(410, 23);
			this.m_lblExampleTitleForSingleChapterBook.Margin = new System.Windows.Forms.Padding(0);
			this.m_lblExampleTitleForSingleChapterBook.Name = "m_lblExampleTitleForSingleChapterBook";
			this.m_lblExampleTitleForSingleChapterBook.Padding = new System.Windows.Forms.Padding(3, 6, 3, 0);
			this.m_lblExampleTitleForSingleChapterBook.Size = new System.Drawing.Size(169, 19);
			this.m_lblExampleTitleForSingleChapterBook.TabIndex = 8;
			this.m_lblExampleTitleForSingleChapterBook.Text = "#";
			this.m_lblExampleTitleForSingleChapterBook.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblExampleTitleForSingleChapterBook, false);
			// 
			// m_lblExampleSingleChapterAnnouncement
			// 
			this.m_lblExampleSingleChapterAnnouncement.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_lblExampleSingleChapterAnnouncement.AutoSize = true;
			this.m_lblExampleSingleChapterAnnouncement.BackColor = System.Drawing.SystemColors.Window;
			this.glyssenColorPalette.SetBackColor(this.m_lblExampleSingleChapterAnnouncement, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_lblExampleSingleChapterAnnouncement, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblExampleSingleChapterAnnouncement, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblExampleSingleChapterAnnouncement, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_lblExampleSingleChapterAnnouncement, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblExampleSingleChapterAnnouncement, "DialogBoxes.ProjectSettingsDlg.ProjectSettingsDlg.m_lblExampleSingleChapterAnnoun" +
        "cement");
			this.m_lblExampleSingleChapterAnnouncement.Location = new System.Drawing.Point(410, 43);
			this.m_lblExampleSingleChapterAnnouncement.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
			this.m_lblExampleSingleChapterAnnouncement.Name = "m_lblExampleSingleChapterAnnouncement";
			this.m_lblExampleSingleChapterAnnouncement.Padding = new System.Windows.Forms.Padding(3, 6, 3, 0);
			this.m_lblExampleSingleChapterAnnouncement.Size = new System.Drawing.Size(169, 19);
			this.m_lblExampleSingleChapterAnnouncement.TabIndex = 9;
			this.m_lblExampleSingleChapterAnnouncement.Text = "#";
			this.m_lblExampleSingleChapterAnnouncement.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblExampleSingleChapterAnnouncement, false);
			// 
			// m_chkAnnounceChaptersForSingleChapterBooks
			// 
			this.m_chkAnnounceChaptersForSingleChapterBooks.AutoSize = true;
			this.m_chkAnnounceChaptersForSingleChapterBooks.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_chkAnnounceChaptersForSingleChapterBooks, Glyssen.Utilities.GlyssenColors.BackColor);
			this.tableLayoutPanel1.SetColumnSpan(this.m_chkAnnounceChaptersForSingleChapterBooks, 2);
			this.m_chkAnnounceChaptersForSingleChapterBooks.Enabled = false;
			this.m_chkAnnounceChaptersForSingleChapterBooks.FlatAppearance.BorderColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_chkAnnounceChaptersForSingleChapterBooks, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_chkAnnounceChaptersForSingleChapterBooks.ForeColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetForeColor(this.m_chkAnnounceChaptersForSingleChapterBooks, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_chkAnnounceChaptersForSingleChapterBooks, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_chkAnnounceChaptersForSingleChapterBooks, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_chkAnnounceChaptersForSingleChapterBooks, "DialogBoxes.ProjectSettingsDlg.ChapterAnnouncementTab.AnnounceChaptersForSingleCh" +
        "apterBooks");
			this.m_chkAnnounceChaptersForSingleChapterBooks.Location = new System.Drawing.Point(3, 121);
			this.m_chkAnnounceChaptersForSingleChapterBooks.Name = "m_chkAnnounceChaptersForSingleChapterBooks";
			this.m_chkAnnounceChaptersForSingleChapterBooks.Size = new System.Drawing.Size(243, 17);
			this.m_chkAnnounceChaptersForSingleChapterBooks.TabIndex = 5;
			this.m_chkAnnounceChaptersForSingleChapterBooks.Text = "Announce Chapters For Single-Chapter Books";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_chkAnnounceChaptersForSingleChapterBooks, true);
			this.m_chkAnnounceChaptersForSingleChapterBooks.UseVisualStyleBackColor = true;
			this.m_chkAnnounceChaptersForSingleChapterBooks.CheckedChanged += new System.EventHandler(this.HandleAnnounceSingleChapterCheckedChanged);
			// 
			// m_lblChapterAnnouncementStyle
			// 
			this.m_lblChapterAnnouncementStyle.AutoSize = true;
			this.m_lblChapterAnnouncementStyle.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblChapterAnnouncementStyle, Glyssen.Utilities.GlyssenColors.BackColor);
			this.tableLayoutPanel1.SetColumnSpan(this.m_lblChapterAnnouncementStyle, 3);
			this.glyssenColorPalette.SetForeColor(this.m_lblChapterAnnouncementStyle, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblChapterAnnouncementStyle.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblChapterAnnouncementStyle, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblChapterAnnouncementStyle, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblChapterAnnouncementStyle, "DialogBoxes.ProjectSettingsDlg.ChapterAnnouncementTab.ChapterAnnouncementStyle");
			this.m_lblChapterAnnouncementStyle.Location = new System.Drawing.Point(0, 3);
			this.m_lblChapterAnnouncementStyle.Margin = new System.Windows.Forms.Padding(0, 3, 0, 6);
			this.m_lblChapterAnnouncementStyle.Name = "m_lblChapterAnnouncementStyle";
			this.m_lblChapterAnnouncementStyle.Size = new System.Drawing.Size(145, 13);
			this.m_lblChapterAnnouncementStyle.TabIndex = 0;
			this.m_lblChapterAnnouncementStyle.Text = "Chapter announcement style:";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblChapterAnnouncementStyle, true);
			// 
			// m_lblBookNameSource
			// 
			this.m_lblBookNameSource.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_lblBookNameSource.AutoSize = true;
			this.m_lblBookNameSource.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblBookNameSource, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_lblBookNameSource, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblBookNameSource.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblBookNameSource, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblBookNameSource, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblBookNameSource, "DialogBoxes.ProjectSettingsDlg.ChapterAnnouncementTab.BookNameSource");
			this.m_lblBookNameSource.Location = new System.Drawing.Point(208, 29);
			this.m_lblBookNameSource.Margin = new System.Windows.Forms.Padding(32, 0, 3, 0);
			this.m_lblBookNameSource.Name = "m_lblBookNameSource";
			this.m_lblBookNameSource.Size = new System.Drawing.Size(112, 13);
			this.m_lblBookNameSource.TabIndex = 8;
			this.m_lblBookNameSource.Text = "Source of book name:";
			this.m_lblBookNameSource.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblBookNameSource, true);
			// 
			// m_rdoBookNamePlusChapterNumber
			// 
			this.m_rdoBookNamePlusChapterNumber.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_rdoBookNamePlusChapterNumber.AutoSize = true;
			this.m_rdoBookNamePlusChapterNumber.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_rdoBookNamePlusChapterNumber, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_rdoBookNamePlusChapterNumber.Checked = true;
			this.m_rdoBookNamePlusChapterNumber.FlatAppearance.BorderColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_rdoBookNamePlusChapterNumber, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_rdoBookNamePlusChapterNumber.ForeColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetForeColor(this.m_rdoBookNamePlusChapterNumber, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_rdoBookNamePlusChapterNumber, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_rdoBookNamePlusChapterNumber, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_rdoBookNamePlusChapterNumber, "DialogBoxes.ProjectSettingsDlg.ChapterAnnouncementTab.BookNamePlusChapterNumberOp" +
        "tion");
			this.m_rdoBookNamePlusChapterNumber.Location = new System.Drawing.Point(3, 27);
			this.m_rdoBookNamePlusChapterNumber.Name = "m_rdoBookNamePlusChapterNumber";
			this.m_rdoBookNamePlusChapterNumber.Size = new System.Drawing.Size(170, 17);
			this.m_rdoBookNamePlusChapterNumber.TabIndex = 1;
			this.m_rdoBookNamePlusChapterNumber.TabStop = true;
			this.m_rdoBookNamePlusChapterNumber.Text = "Book Name + Chapter Number";
			this.m_rdoBookNamePlusChapterNumber.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_rdoBookNamePlusChapterNumber, true);
			this.m_rdoBookNamePlusChapterNumber.UseVisualStyleBackColor = true;
			this.m_rdoBookNamePlusChapterNumber.CheckedChanged += new System.EventHandler(this.HandleChapterAnnouncementStyleChange);
			// 
			// m_lblExample
			// 
			this.m_lblExample.AutoSize = true;
			this.m_lblExample.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblExample, Glyssen.Utilities.GlyssenColors.BackColor);
			this.tableLayoutPanel1.SetColumnSpan(this.m_lblExample, 3);
			this.m_lblExample.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.glyssenColorPalette.SetForeColor(this.m_lblExample, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblExample.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblExample, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblExample, "");
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblExample, "DialogBoxes.ProjectSettingsDlg.ChapterAnnouncementTab.Examples");
			this.m_lblExample.Location = new System.Drawing.Point(3, 153);
			this.m_lblExample.Margin = new System.Windows.Forms.Padding(3, 12, 3, 0);
			this.m_lblExample.Name = "m_lblExample";
			this.m_lblExample.Size = new System.Drawing.Size(64, 13);
			this.m_lblExample.TabIndex = 6;
			this.m_lblExample.Text = "Examples:";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblExample, true);
			// 
			// m_rdoChapterLabel
			// 
			this.m_rdoChapterLabel.AutoSize = true;
			this.m_rdoChapterLabel.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_rdoChapterLabel, Glyssen.Utilities.GlyssenColors.BackColor);
			this.tableLayoutPanel1.SetColumnSpan(this.m_rdoChapterLabel, 2);
			this.m_rdoChapterLabel.FlatAppearance.BorderColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_rdoChapterLabel, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_rdoChapterLabel.ForeColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetForeColor(this.m_rdoChapterLabel, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_rdoChapterLabel, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_rdoChapterLabel, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_rdoChapterLabel, "DialogBoxes.ProjectSettingsDlg.ChapterAnnouncementTab.ChapterLabelOption");
			this.m_rdoChapterLabel.Location = new System.Drawing.Point(3, 52);
			this.m_rdoChapterLabel.Name = "m_rdoChapterLabel";
			this.m_rdoChapterLabel.Size = new System.Drawing.Size(214, 17);
			this.m_rdoChapterLabel.TabIndex = 2;
			this.m_rdoChapterLabel.TabStop = true;
			this.m_rdoChapterLabel.Text = "Use Chapter Labels from \\cl field in data";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_rdoChapterLabel, true);
			this.m_rdoChapterLabel.UseVisualStyleBackColor = true;
			this.m_rdoChapterLabel.CheckedChanged += new System.EventHandler(this.HandleChapterAnnouncementStyleChange);
			// 
			// m_rdoCustom
			// 
			this.m_rdoCustom.AutoSize = true;
			this.m_rdoCustom.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_rdoCustom, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_rdoCustom.FlatAppearance.BorderColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_rdoCustom, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_rdoCustom.ForeColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetForeColor(this.m_rdoCustom, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_rdoCustom, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_rdoCustom, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_rdoCustom, "DialogBoxes.ProjectSettingsDlg.ChapterAnnouncementTab.CustomOption");
			this.m_rdoCustom.Location = new System.Drawing.Point(3, 75);
			this.m_rdoCustom.Name = "m_rdoCustom";
			this.m_rdoCustom.Size = new System.Drawing.Size(60, 17);
			this.m_rdoCustom.TabIndex = 3;
			this.m_rdoCustom.TabStop = true;
			this.m_rdoCustom.Text = "Custom";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_rdoCustom, true);
			this.m_rdoCustom.UseVisualStyleBackColor = true;
			this.m_rdoCustom.Visible = false;
			// 
			// m_chkChapterOneAnnouncements
			// 
			this.m_chkChapterOneAnnouncements.AutoSize = true;
			this.m_chkChapterOneAnnouncements.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_chkChapterOneAnnouncements, Glyssen.Utilities.GlyssenColors.BackColor);
			this.tableLayoutPanel1.SetColumnSpan(this.m_chkChapterOneAnnouncements, 2);
			this.m_chkChapterOneAnnouncements.FlatAppearance.BorderColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_chkChapterOneAnnouncements, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_chkChapterOneAnnouncements.ForeColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetForeColor(this.m_chkChapterOneAnnouncements, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_chkChapterOneAnnouncements, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_chkChapterOneAnnouncements, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_chkChapterOneAnnouncements, "DialogBoxes.ProjectSettingsDlg.ChapterAnnouncementTab.AnnounceFirstChapterInBook");
			this.m_chkChapterOneAnnouncements.Location = new System.Drawing.Point(3, 98);
			this.m_chkChapterOneAnnouncements.Name = "m_chkChapterOneAnnouncements";
			this.m_chkChapterOneAnnouncements.Size = new System.Drawing.Size(176, 17);
			this.m_chkChapterOneAnnouncements.TabIndex = 4;
			this.m_chkChapterOneAnnouncements.Text = "Announce First Chapter in Book";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_chkChapterOneAnnouncements, true);
			this.m_chkChapterOneAnnouncements.UseVisualStyleBackColor = true;
			this.m_chkChapterOneAnnouncements.CheckedChanged += new System.EventHandler(this.HandleAnnounceFirstChapterCheckedChanged);
			// 
			// m_cboBookMarker
			// 
			this.m_cboBookMarker.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.glyssenColorPalette.SetBackColor(this.m_cboBookMarker, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_cboBookMarker.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.glyssenColorPalette.SetForeColor(this.m_cboBookMarker, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_cboBookMarker.FormattingEnabled = true;
			this.m_cboBookMarker.Items.AddRange(new object[] {
            "Page header (\\h)",
            "Main Title (\\mt1)",
            "Short name",
            "Long name"});
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_cboBookMarker, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_cboBookMarker, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_cboBookMarker, "DialogBoxes.ProjectSettingsDlg.ChapterAnnouncementTab.BookMarkerComboBox");
			this.m_cboBookMarker.Location = new System.Drawing.Point(326, 25);
			this.m_cboBookMarker.MinimumSize = new System.Drawing.Size(150, 0);
			this.m_cboBookMarker.Name = "m_cboBookMarker";
			this.m_cboBookMarker.Size = new System.Drawing.Size(257, 21);
			this.m_cboBookMarker.TabIndex = 9;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_cboBookMarker, false);
			this.m_cboBookMarker.SelectedIndexChanged += new System.EventHandler(this.HandleChapterAnnouncementChange);
			// 
			// m_lblChapterAnnouncementWarning
			// 
			this.m_lblChapterAnnouncementWarning.AutoSize = true;
			this.m_lblChapterAnnouncementWarning.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblChapterAnnouncementWarning, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_lblChapterAnnouncementWarning, Glyssen.Utilities.GlyssenColors.Warning);
			this.m_lblChapterAnnouncementWarning.ForeColor = System.Drawing.Color.Red;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblChapterAnnouncementWarning, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblChapterAnnouncementWarning, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblChapterAnnouncementWarning, "DialogBoxes.ProjectSettingsDlg.ChapterAnnouncementTab.ChapterAnnouncementWarning");
			this.m_lblChapterAnnouncementWarning.Location = new System.Drawing.Point(326, 59);
			this.m_lblChapterAnnouncementWarning.Margin = new System.Windows.Forms.Padding(3, 10, 3, 0);
			this.m_lblChapterAnnouncementWarning.Name = "m_lblChapterAnnouncementWarning";
			this.tableLayoutPanel1.SetRowSpan(this.m_lblChapterAnnouncementWarning, 4);
			this.m_lblChapterAnnouncementWarning.Size = new System.Drawing.Size(254, 39);
			this.m_lblChapterAnnouncementWarning.TabIndex = 10;
			this.m_lblChapterAnnouncementWarning.Text = "It looks like the current style will result in chapter announcements that do not " +
    "include a book name or a vernacular word for \"chapter.\"";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblChapterAnnouncementWarning, true);
			this.m_lblChapterAnnouncementWarning.Visible = false;
			// 
			// m_tabPageReferenceTexts
			// 
			this.glyssenColorPalette.SetBackColor(this.m_tabPageReferenceTexts, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tabPageReferenceTexts.BackColor = System.Drawing.SystemColors.Control;
			this.m_tabPageReferenceTexts.Controls.Add(this.m_tableLayoutReferenceTexts);
			this.glyssenColorPalette.SetForeColor(this.m_tabPageReferenceTexts, Glyssen.Utilities.GlyssenColors.Default);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_tabPageReferenceTexts, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_tabPageReferenceTexts, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_tabPageReferenceTexts, "DialogBoxes.ProjectSettingsDlg.ReferenceTextsTab.TabName");
			this.m_tabPageReferenceTexts.Location = new System.Drawing.Point(4, 22);
			this.m_tabPageReferenceTexts.Name = "m_tabPageReferenceTexts";
			this.m_tabPageReferenceTexts.Padding = new System.Windows.Forms.Padding(7, 12, 7, 12);
			this.m_tabPageReferenceTexts.Size = new System.Drawing.Size(603, 337);
			this.m_tabPageReferenceTexts.TabIndex = 3;
			this.m_tabPageReferenceTexts.Text = "Reference Text";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_tabPageReferenceTexts, true);
			// 
			// m_tableLayoutReferenceTexts
			// 
			this.m_tableLayoutReferenceTexts.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_tableLayoutReferenceTexts, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutReferenceTexts.ColumnCount = 3;
			this.m_tableLayoutReferenceTexts.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.m_tableLayoutReferenceTexts.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.m_tableLayoutReferenceTexts.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tableLayoutReferenceTexts.Controls.Add(this.m_labelReferenceText, 0, 0);
			this.m_tableLayoutReferenceTexts.Controls.Add(this.m_ReferenceText, 1, 0);
			this.m_tableLayoutReferenceTexts.Controls.Add(this.m_referenceTextExplanation, 1, 1);
			this.m_tableLayoutReferenceTexts.Controls.Add(this.m_linkRefTextAttribution, 2, 0);
			this.m_tableLayoutReferenceTexts.Dock = System.Windows.Forms.DockStyle.Fill;
			this.glyssenColorPalette.SetForeColor(this.m_tableLayoutReferenceTexts, Glyssen.Utilities.GlyssenColors.Default);
			this.m_tableLayoutReferenceTexts.Location = new System.Drawing.Point(7, 12);
			this.m_tableLayoutReferenceTexts.Name = "m_tableLayoutReferenceTexts";
			this.m_tableLayoutReferenceTexts.Padding = new System.Windows.Forms.Padding(3);
			this.m_tableLayoutReferenceTexts.RowCount = 2;
			this.m_tableLayoutReferenceTexts.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutReferenceTexts.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutReferenceTexts.Size = new System.Drawing.Size(589, 313);
			this.m_tableLayoutReferenceTexts.TabIndex = 0;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_tableLayoutReferenceTexts, false);
			// 
			// m_labelReferenceText
			// 
			this.m_labelReferenceText.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_labelReferenceText.AutoSize = true;
			this.m_labelReferenceText.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_labelReferenceText, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_labelReferenceText, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_labelReferenceText.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_labelReferenceText, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_labelReferenceText, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_labelReferenceText, "DialogBoxes.ProjectSettingsDlg.ReferenceTextTab.ReferenceText");
			this.m_labelReferenceText.Location = new System.Drawing.Point(6, 10);
			this.m_labelReferenceText.Name = "m_labelReferenceText";
			this.m_labelReferenceText.Size = new System.Drawing.Size(110, 13);
			this.m_labelReferenceText.TabIndex = 15;
			this.m_labelReferenceText.Text = "Main Reference Text:";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_labelReferenceText, true);
			// 
			// m_ReferenceText
			// 
			this.m_ReferenceText.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.glyssenColorPalette.SetBackColor(this.m_ReferenceText, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_ReferenceText.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.glyssenColorPalette.SetForeColor(this.m_ReferenceText, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_ReferenceText.FormattingEnabled = true;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_ReferenceText, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_ReferenceText, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_ReferenceText, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_ReferenceText, "DialogBoxes.ProjectSettingsDlg.ReferenceTextDropdown");
			this.m_ReferenceText.Location = new System.Drawing.Point(125, 3);
			this.m_ReferenceText.Margin = new System.Windows.Forms.Padding(6, 0, 6, 6);
			this.m_ReferenceText.Name = "m_ReferenceText";
			this.m_ReferenceText.Size = new System.Drawing.Size(121, 21);
			this.m_ReferenceText.TabIndex = 16;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_ReferenceText, false);
			this.m_ReferenceText.SelectedIndexChanged += new System.EventHandler(this.HandleSelectedReferenceTextChanged);
			// 
			// m_referenceTextExplanation
			// 
			this.m_referenceTextExplanation.AutoSize = true;
			this.m_referenceTextExplanation.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_referenceTextExplanation, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutReferenceTexts.SetColumnSpan(this.m_referenceTextExplanation, 2);
			this.m_referenceTextExplanation.Dock = System.Windows.Forms.DockStyle.Top;
			this.glyssenColorPalette.SetForeColor(this.m_referenceTextExplanation, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_referenceTextExplanation.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_referenceTextExplanation, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_referenceTextExplanation, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_referenceTextExplanation, "DialogBoxes.ProjectSettingsDlg.ReferenceTextTab.ReferenceTextExplanation");
			this.m_referenceTextExplanation.Location = new System.Drawing.Point(122, 36);
			this.m_referenceTextExplanation.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
			this.m_referenceTextExplanation.Name = "m_referenceTextExplanation";
			this.m_referenceTextExplanation.Size = new System.Drawing.Size(461, 26);
			this.m_referenceTextExplanation.TabIndex = 17;
			this.m_referenceTextExplanation.Text = "If you choose a main reference text other than English, a secondary reference tex" +
    "t (English) will also be added to the recording script.";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_referenceTextExplanation, true);
			// 
			// m_tabPageScriptOptions
			// 
			this.glyssenColorPalette.SetBackColor(this.m_tabPageScriptOptions, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tabPageScriptOptions.BackColor = System.Drawing.SystemColors.Control;
			this.m_tabPageScriptOptions.Controls.Add(this.m_tableLayoutScriptOptions);
			this.glyssenColorPalette.SetForeColor(this.m_tabPageScriptOptions, Glyssen.Utilities.GlyssenColors.Default);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_tabPageScriptOptions, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_tabPageScriptOptions, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_tabPageScriptOptions, "DialogBoxes.ProjectSettingsDlg.ScriptOptionsTab.TabName");
			this.m_tabPageScriptOptions.Location = new System.Drawing.Point(4, 22);
			this.m_tabPageScriptOptions.Name = "m_tabPageScriptOptions";
			this.m_tabPageScriptOptions.Padding = new System.Windows.Forms.Padding(7, 12, 7, 12);
			this.m_tabPageScriptOptions.Size = new System.Drawing.Size(603, 337);
			this.m_tabPageScriptOptions.TabIndex = 4;
			this.m_tabPageScriptOptions.Text = "Script Options";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_tabPageScriptOptions, true);
			// 
			// m_tableLayoutScriptOptions
			// 
			this.glyssenColorPalette.SetBackColor(this.m_tableLayoutScriptOptions, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutScriptOptions.ColumnCount = 3;
			this.m_tableLayoutScriptOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.m_tableLayoutScriptOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.m_tableLayoutScriptOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tableLayoutScriptOptions.Controls.Add(this.m_labelBookIntro, 0, 0);
			this.m_tableLayoutScriptOptions.Controls.Add(this.m_bookIntro, 1, 0);
			this.m_tableLayoutScriptOptions.Controls.Add(this.m_labelSectionHeadings, 0, 1);
			this.m_tableLayoutScriptOptions.Controls.Add(this.m_labelTitleChapter, 0, 2);
			this.m_tableLayoutScriptOptions.Controls.Add(this.m_sectionHeadings, 1, 1);
			this.m_tableLayoutScriptOptions.Controls.Add(this.m_titleChapters, 1, 2);
			this.m_tableLayoutScriptOptions.Dock = System.Windows.Forms.DockStyle.Fill;
			this.glyssenColorPalette.SetForeColor(this.m_tableLayoutScriptOptions, Glyssen.Utilities.GlyssenColors.Default);
			this.m_tableLayoutScriptOptions.Location = new System.Drawing.Point(7, 12);
			this.m_tableLayoutScriptOptions.Name = "m_tableLayoutScriptOptions";
			this.m_tableLayoutScriptOptions.Padding = new System.Windows.Forms.Padding(3);
			this.m_tableLayoutScriptOptions.RowCount = 4;
			this.m_tableLayoutScriptOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutScriptOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutScriptOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutScriptOptions.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tableLayoutScriptOptions.Size = new System.Drawing.Size(589, 313);
			this.m_tableLayoutScriptOptions.TabIndex = 1;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_tableLayoutScriptOptions, false);
			// 
			// m_labelBookIntro
			// 
			this.m_labelBookIntro.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_labelBookIntro.AutoSize = true;
			this.m_labelBookIntro.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_labelBookIntro, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_labelBookIntro, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_labelBookIntro.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_labelBookIntro, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_labelBookIntro, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_labelBookIntro, "DialogBoxes.ProjectSettingsDlg.ScriptOptionsTab.BookIntroduction");
			this.m_labelBookIntro.Location = new System.Drawing.Point(6, 10);
			this.m_labelBookIntro.Name = "m_labelBookIntro";
			this.m_labelBookIntro.Size = new System.Drawing.Size(91, 13);
			this.m_labelBookIntro.TabIndex = 15;
			this.m_labelBookIntro.Text = "Book Introduction";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_labelBookIntro, true);
			// 
			// m_bookIntro
			// 
			this.m_bookIntro.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.glyssenColorPalette.SetBackColor(this.m_bookIntro, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_bookIntro.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.glyssenColorPalette.SetForeColor(this.m_bookIntro, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_bookIntro.FormattingEnabled = true;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_bookIntro, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_bookIntro, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_bookIntro, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_bookIntro, "DialogBoxes.ProjectSettingsDlg.ReferenceTextDropdown");
			this.m_bookIntro.Location = new System.Drawing.Point(117, 3);
			this.m_bookIntro.Margin = new System.Windows.Forms.Padding(6, 0, 6, 6);
			this.m_bookIntro.Name = "m_bookIntro";
			this.m_bookIntro.Size = new System.Drawing.Size(121, 21);
			this.m_bookIntro.TabIndex = 16;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_bookIntro, false);
			// 
			// m_labelSectionHeadings
			// 
			this.m_labelSectionHeadings.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_labelSectionHeadings.AutoSize = true;
			this.m_labelSectionHeadings.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_labelSectionHeadings, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_labelSectionHeadings, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_labelSectionHeadings.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_labelSectionHeadings, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_labelSectionHeadings, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_labelSectionHeadings, "DialogBoxes.ProjectSettingsDlg.ScriptOptionsTab.SectionHeadings");
			this.m_labelSectionHeadings.Location = new System.Drawing.Point(6, 40);
			this.m_labelSectionHeadings.Name = "m_labelSectionHeadings";
			this.m_labelSectionHeadings.Size = new System.Drawing.Size(91, 13);
			this.m_labelSectionHeadings.TabIndex = 17;
			this.m_labelSectionHeadings.Text = "Section Headings";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_labelSectionHeadings, true);
			// 
			// m_labelTitleChapter
			// 
			this.m_labelTitleChapter.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_labelTitleChapter.AutoSize = true;
			this.m_labelTitleChapter.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_labelTitleChapter, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_labelTitleChapter, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_labelTitleChapter.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_labelTitleChapter, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_labelTitleChapter, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_labelTitleChapter, "DialogBoxes.ProjectSettingsDlg.ScriptOptionsTab.BookTitleChapter");
			this.m_labelTitleChapter.Location = new System.Drawing.Point(6, 73);
			this.m_labelTitleChapter.Name = "m_labelTitleChapter";
			this.m_labelTitleChapter.Size = new System.Drawing.Size(102, 13);
			this.m_labelTitleChapter.TabIndex = 18;
			this.m_labelTitleChapter.Text = "Book Title/Chapters";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_labelTitleChapter, true);
			// 
			// m_sectionHeadings
			// 
			this.m_sectionHeadings.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.glyssenColorPalette.SetBackColor(this.m_sectionHeadings, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_sectionHeadings.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.glyssenColorPalette.SetForeColor(this.m_sectionHeadings, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_sectionHeadings.FormattingEnabled = true;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_sectionHeadings, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_sectionHeadings, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_sectionHeadings, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_sectionHeadings, "DialogBoxes.ProjectSettingsDlg.ReferenceTextDropdown");
			this.m_sectionHeadings.Location = new System.Drawing.Point(117, 36);
			this.m_sectionHeadings.Margin = new System.Windows.Forms.Padding(6);
			this.m_sectionHeadings.Name = "m_sectionHeadings";
			this.m_sectionHeadings.Size = new System.Drawing.Size(121, 21);
			this.m_sectionHeadings.TabIndex = 19;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_sectionHeadings, false);
			// 
			// m_titleChapters
			// 
			this.m_titleChapters.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.glyssenColorPalette.SetBackColor(this.m_titleChapters, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_titleChapters.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.glyssenColorPalette.SetForeColor(this.m_titleChapters, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_titleChapters.FormattingEnabled = true;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_titleChapters, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_titleChapters, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_titleChapters, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_titleChapters, "DialogBoxes.ProjectSettingsDlg.ReferenceTextDropdown");
			this.m_titleChapters.Location = new System.Drawing.Point(117, 69);
			this.m_titleChapters.Margin = new System.Windows.Forms.Padding(6);
			this.m_titleChapters.Name = "m_titleChapters";
			this.m_titleChapters.Size = new System.Drawing.Size(121, 21);
			this.m_titleChapters.TabIndex = 20;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_titleChapters, false);
			// 
			// m_tabControl
			// 
			this.m_tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.glyssenColorPalette.SetBackColor(this.m_tabControl, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tabControl.Controls.Add(this.m_tabPageGeneral);
			this.m_tabControl.Controls.Add(this.m_tabPageWritingSystem);
			this.m_tabControl.Controls.Add(this.m_tabPageTitleAndChapterAnnouncmentOptions);
			this.m_tabControl.Controls.Add(this.m_tabPageReferenceTexts);
			this.m_tabControl.Controls.Add(this.m_tabPageScriptOptions);
			this.glyssenColorPalette.SetForeColor(this.m_tabControl, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_tabControl.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_tabControl.Location = new System.Drawing.Point(7, 14);
			this.m_tabControl.Name = "m_tabControl";
			this.m_tabControl.SelectedIndex = 0;
			this.m_tabControl.Size = new System.Drawing.Size(611, 363);
			this.m_tabControl.TabIndex = 0;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_tabControl, true);
			this.m_tabControl.SelectedIndexChanged += new System.EventHandler(this.HandleSelectedTabPageChanged);
			// 
			// m_lblRefTextAttribution
			// 
			this.m_linkRefTextAttribution.AutoSize = true;
			this.glyssenColorPalette.SetBackColor(this.m_linkRefTextAttribution, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_linkRefTextAttribution, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_linkRefTextAttribution, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_linkRefTextAttribution, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_linkRefTextAttribution, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_linkRefTextAttribution, "DialogBoxes.ProjectSettingsDlg.m_linkRefTextAttribution");
			this.m_linkRefTextAttribution.Location = new System.Drawing.Point(255, 3);
			this.m_linkRefTextAttribution.Name = "m_linkRefTextAttribution";
			this.m_linkRefTextAttribution.Size = new System.Drawing.Size(14, 13);
			this.m_linkRefTextAttribution.TabIndex = 18;
			this.m_linkRefTextAttribution.Text = "#";
			this.m_linkRefTextAttribution.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.HandleWebSiteLinkClicked);
			this.glyssenColorPalette.SetUsePaletteColors(this.m_linkRefTextAttribution, true);
			// 
			// ProjectSettingsDlg
			// 
			this.AcceptButton = this.m_btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.glyssenColorPalette.SetBackColor(this, Glyssen.Utilities.GlyssenColors.BackColor);
			this.CancelButton = this.m_btnCancel;
			this.ClientSize = new System.Drawing.Size(636, 422);
			this.Controls.Add(this.m_tabControl);
			this.Controls.Add(this.m_btnOk);
			this.Controls.Add(this.m_btnCancel);
			this.glyssenColorPalette.SetForeColor(this, Glyssen.Utilities.GlyssenColors.Default);
			this.Icon = global::Glyssen.Properties.Resources.glyssenIcon;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this, null);
			this.m_l10NSharpExtender.SetLocalizingId(this, "DialogBoxes.ProjectSettingsDlg.WindowTitle");
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(644, 460);
			this.Name = "ProjectSettingsDlg";
			this.Padding = new System.Windows.Forms.Padding(15, 15, 15, 55);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Project Settings";
			this.glyssenColorPalette.SetUsePaletteColors(this, true);
			this.Load += new System.EventHandler(this.ProjectSettingsDlg_Load);
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).EndInit();
			this.m_tabPageGeneral.ResumeLayout(false);
			this.m_tableLayoutMain.ResumeLayout(false);
			this.m_tableLayoutMain.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.m_tabPageWritingSystem.ResumeLayout(false);
			this.m_tabPageTitleAndChapterAnnouncmentOptions.ResumeLayout(false);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.m_tableLayoutPanelAnnouncmentsExample.ResumeLayout(false);
			this.m_tableLayoutPanelAnnouncmentsExample.PerformLayout();
			this.m_tabPageReferenceTexts.ResumeLayout(false);
			this.m_tableLayoutReferenceTexts.ResumeLayout(false);
			this.m_tableLayoutReferenceTexts.PerformLayout();
			this.m_tabPageScriptOptions.ResumeLayout(false);
			this.m_tableLayoutScriptOptions.ResumeLayout(false);
			this.m_tableLayoutScriptOptions.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.glyssenColorPalette)).EndInit();
			this.m_tabControl.ResumeLayout(false);
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
		private System.Windows.Forms.TabControl m_tabControl;
		private System.Windows.Forms.TabPage m_tabPageGeneral;
		private System.Windows.Forms.TabPage m_tabPageWritingSystem;
		private System.Windows.Forms.TabPage m_tabPageTitleAndChapterAnnouncmentOptions;
		private System.Windows.Forms.Label m_lblChapterAnnouncementStyle;
		private System.Windows.Forms.RadioButton m_rdoBookNamePlusChapterNumber;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label m_lblBookNameSource;
		private System.Windows.Forms.ComboBox m_cboBookMarker;
		private System.Windows.Forms.Label m_lblExample;
		private System.Windows.Forms.RadioButton m_rdoChapterLabel;
		private System.Windows.Forms.RadioButton m_rdoCustom;
		private System.Windows.Forms.Label m_lblExampleSingleChapterAnnouncement;
		private System.Windows.Forms.Label m_lblExampleTitleForSingleChapterBook;
		private System.Windows.Forms.Label m_lblExampleSubsequentChapterAnnouncement;
		private System.Windows.Forms.Label m_lblExampleFirstChapterAnnouncement;
		private System.Windows.Forms.Label m_lblChapterAnnouncement;
		private System.Windows.Forms.Label m_lblSingleChapterBookExample;
		private System.Windows.Forms.Label m_lblSubsequentChaptersExample;
		private System.Windows.Forms.Label m_lblFirstChapterExample;
		private System.Windows.Forms.Label m_lblBookTitleHeading;
		private System.Windows.Forms.Label m_lblExampleTitleForMultipleChapterBook;
		private System.Windows.Forms.CheckBox m_chkAnnounceChaptersForSingleChapterBooks;
		private System.Windows.Forms.CheckBox m_chkChapterOneAnnouncements;
		private System.Windows.Forms.TableLayoutPanel m_tableLayoutPanelAnnouncmentsExample;
		private System.Windows.Forms.Label m_lblChapterAnnouncementWarning;
        private System.Windows.Forms.Label m_lblAudioStockNumber;
        private System.Windows.Forms.TextBox m_txtAudioStockNumber;
		private System.Windows.Forms.TabPage m_tabPageReferenceTexts;
		private System.Windows.Forms.TableLayoutPanel m_tableLayoutReferenceTexts;
		private System.Windows.Forms.Label m_labelReferenceText;
		private System.Windows.Forms.ComboBox m_ReferenceText;
		private System.Windows.Forms.TabPage m_tabPageScriptOptions;
		private System.Windows.Forms.TableLayoutPanel m_tableLayoutScriptOptions;
		private System.Windows.Forms.Label m_labelBookIntro;
		private System.Windows.Forms.ComboBox m_bookIntro;
		private System.Windows.Forms.Label m_labelSectionHeadings;
		private System.Windows.Forms.Label m_labelTitleChapter;
		private System.Windows.Forms.ComboBox m_sectionHeadings;
		private System.Windows.Forms.ComboBox m_titleChapters;
		public System.Windows.Forms.Label m_referenceTextExplanation;
		private System.Windows.Forms.LinkLabel m_linkRefTextAttribution;
	}
}