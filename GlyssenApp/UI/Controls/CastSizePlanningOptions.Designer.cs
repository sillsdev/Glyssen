using GlyssenApp.Utilities;

namespace GlyssenApp.UI.Controls
{
	partial class CastSizePlanningOptions
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
			if (disposing)
			{
				if (components != null)
					components.Dispose();

				m_viewModel.CastSizeRowValuesChanged -= m_viewModel_CastSizeRowValuesChanged;
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
			this.m_tableLayout = new System.Windows.Forms.TableLayoutPanel();
			this.m_rbMatchVoiceActorList = new System.Windows.Forms.RadioButton();
			this.m_rbSmall = new System.Windows.Forms.RadioButton();
			this.m_rbRecommended = new System.Windows.Forms.RadioButton();
			this.m_rbLarge = new System.Windows.Forms.RadioButton();
			this.m_rbCustom = new System.Windows.Forms.RadioButton();
			this.m_lblCastSize = new System.Windows.Forms.Label();
			this.m_lblMen = new System.Windows.Forms.Label();
			this.m_lblWomen = new System.Windows.Forms.Label();
			this.m_lblTotal = new System.Windows.Forms.Label();
			this.m_lblChildren = new System.Windows.Forms.Label();
			this.m_lblSmall = new System.Windows.Forms.Label();
			this.m_lblRecommended = new System.Windows.Forms.Label();
			this.m_lblLarge = new System.Windows.Forms.Label();
			this.m_lblCustom = new System.Windows.Forms.Label();
			this.m_lblMatchList = new System.Windows.Forms.Label();
			this.m_lblSmallMen = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.label12 = new System.Windows.Forms.Label();
			this.label13 = new System.Windows.Forms.Label();
			this.label14 = new System.Windows.Forms.Label();
			this.label15 = new System.Windows.Forms.Label();
			this.label16 = new System.Windows.Forms.Label();
			this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
			this.numericUpDown2 = new System.Windows.Forms.NumericUpDown();
			this.numericUpDown3 = new System.Windows.Forms.NumericUpDown();
			this.glyssenColorPalette = new GlyssenColorPalette();
			this.m_l10NSharpExtender = new L10NSharp.UI.L10NSharpExtender(this.components);
			this.m_tableLayout.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown3)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.glyssenColorPalette)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).BeginInit();
			this.SuspendLayout();
			// 
			// m_tableLayout
			// 
			this.m_tableLayout.AutoSize = true;
			this.m_tableLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.glyssenColorPalette.SetBackColor(this.m_tableLayout, GlyssenColors.BackColor);
			this.m_tableLayout.ColumnCount = 6;
			this.m_tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.m_tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.m_tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.m_tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.m_tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.m_tableLayout.Controls.Add(this.m_rbMatchVoiceActorList, 0, 5);
			this.m_tableLayout.Controls.Add(this.m_rbSmall, 0, 1);
			this.m_tableLayout.Controls.Add(this.m_rbRecommended, 0, 2);
			this.m_tableLayout.Controls.Add(this.m_rbLarge, 0, 3);
			this.m_tableLayout.Controls.Add(this.m_rbCustom, 0, 4);
			this.m_tableLayout.Controls.Add(this.m_lblCastSize, 1, 0);
			this.m_tableLayout.Controls.Add(this.m_lblMen, 2, 0);
			this.m_tableLayout.Controls.Add(this.m_lblWomen, 3, 0);
			this.m_tableLayout.Controls.Add(this.m_lblTotal, 5, 0);
			this.m_tableLayout.Controls.Add(this.m_lblChildren, 4, 0);
			this.m_tableLayout.Controls.Add(this.m_lblSmall, 1, 1);
			this.m_tableLayout.Controls.Add(this.m_lblRecommended, 1, 2);
			this.m_tableLayout.Controls.Add(this.m_lblLarge, 1, 3);
			this.m_tableLayout.Controls.Add(this.m_lblCustom, 1, 4);
			this.m_tableLayout.Controls.Add(this.m_lblMatchList, 1, 5);
			this.m_tableLayout.Controls.Add(this.m_lblSmallMen, 2, 1);
			this.m_tableLayout.Controls.Add(this.label1, 3, 1);
			this.m_tableLayout.Controls.Add(this.label2, 4, 1);
			this.m_tableLayout.Controls.Add(this.label3, 5, 1);
			this.m_tableLayout.Controls.Add(this.label4, 2, 2);
			this.m_tableLayout.Controls.Add(this.label5, 3, 2);
			this.m_tableLayout.Controls.Add(this.label6, 4, 2);
			this.m_tableLayout.Controls.Add(this.label7, 5, 2);
			this.m_tableLayout.Controls.Add(this.label8, 2, 3);
			this.m_tableLayout.Controls.Add(this.label9, 3, 3);
			this.m_tableLayout.Controls.Add(this.label10, 4, 3);
			this.m_tableLayout.Controls.Add(this.label11, 5, 3);
			this.m_tableLayout.Controls.Add(this.label12, 5, 4);
			this.m_tableLayout.Controls.Add(this.label13, 5, 5);
			this.m_tableLayout.Controls.Add(this.label14, 4, 5);
			this.m_tableLayout.Controls.Add(this.label15, 3, 5);
			this.m_tableLayout.Controls.Add(this.label16, 2, 5);
			this.m_tableLayout.Controls.Add(this.numericUpDown1, 2, 4);
			this.m_tableLayout.Controls.Add(this.numericUpDown2, 3, 4);
			this.m_tableLayout.Controls.Add(this.numericUpDown3, 4, 4);
			this.m_tableLayout.Dock = System.Windows.Forms.DockStyle.Top;
			this.glyssenColorPalette.SetForeColor(this.m_tableLayout, GlyssenColors.ForeColor);
			this.m_tableLayout.Location = new System.Drawing.Point(0, 0);
			this.m_tableLayout.Margin = new System.Windows.Forms.Padding(0);
			this.m_tableLayout.Name = "m_tableLayout";
			this.m_tableLayout.RowCount = 6;
			this.m_tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayout.Size = new System.Drawing.Size(607, 151);
			this.m_tableLayout.TabIndex = 0;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_tableLayout, false);
			this.m_tableLayout.CellPaint += new System.Windows.Forms.TableLayoutCellPaintEventHandler(this.m_tableLayout_CellPaint);
			this.m_tableLayout.Resize += new System.EventHandler(this.m_tableLayout_Resize);
			// 
			// m_rbMatchVoiceActorList
			// 
			this.m_rbMatchVoiceActorList.AutoSize = true;
			this.glyssenColorPalette.SetBackColor(this.m_rbMatchVoiceActorList, GlyssenColors.BackColor);
			this.m_rbMatchVoiceActorList.BackColor = System.Drawing.SystemColors.Control;
			this.m_rbMatchVoiceActorList.FlatAppearance.BorderColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_rbMatchVoiceActorList, GlyssenColors.ForeColor);
			this.m_rbMatchVoiceActorList.ForeColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetForeColor(this.m_rbMatchVoiceActorList, GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_rbMatchVoiceActorList, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_rbMatchVoiceActorList, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_rbMatchVoiceActorList, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_rbMatchVoiceActorList, "DialogBoxes.CastSizePlanningOptions.radioButton5");
			this.m_rbMatchVoiceActorList.Location = new System.Drawing.Point(6, 132);
			this.m_rbMatchVoiceActorList.Margin = new System.Windows.Forms.Padding(6, 6, 4, 6);
			this.m_rbMatchVoiceActorList.Name = "m_rbMatchVoiceActorList";
			this.m_rbMatchVoiceActorList.Size = new System.Drawing.Size(14, 13);
			this.m_rbMatchVoiceActorList.TabIndex = 4;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_rbMatchVoiceActorList, true);
			this.m_rbMatchVoiceActorList.UseVisualStyleBackColor = true;
			this.m_rbMatchVoiceActorList.CheckedChanged += new System.EventHandler(this.OptionCheckedChanged);
			// 
			// m_rbSmall
			// 
			this.m_rbSmall.AutoSize = true;
			this.glyssenColorPalette.SetBackColor(this.m_rbSmall, GlyssenColors.BackColor);
			this.m_rbSmall.BackColor = System.Drawing.SystemColors.Control;
			this.m_rbSmall.FlatAppearance.BorderColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_rbSmall, GlyssenColors.ForeColor);
			this.m_rbSmall.ForeColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetForeColor(this.m_rbSmall, GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_rbSmall, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_rbSmall, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_rbSmall, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_rbSmall, "DialogBoxes.CastSizePlanningOptions.radioButton1");
			this.m_rbSmall.Location = new System.Drawing.Point(6, 31);
			this.m_rbSmall.Margin = new System.Windows.Forms.Padding(6, 6, 4, 6);
			this.m_rbSmall.Name = "m_rbSmall";
			this.m_rbSmall.Size = new System.Drawing.Size(14, 13);
			this.m_rbSmall.TabIndex = 0;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_rbSmall, true);
			this.m_rbSmall.UseVisualStyleBackColor = true;
			this.m_rbSmall.CheckedChanged += new System.EventHandler(this.OptionCheckedChanged);
			// 
			// m_rbRecommended
			// 
			this.m_rbRecommended.AutoSize = true;
			this.glyssenColorPalette.SetBackColor(this.m_rbRecommended, GlyssenColors.BackColor);
			this.m_rbRecommended.BackColor = System.Drawing.SystemColors.Control;
			this.m_rbRecommended.FlatAppearance.BorderColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_rbRecommended, GlyssenColors.ForeColor);
			this.m_rbRecommended.ForeColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetForeColor(this.m_rbRecommended, GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_rbRecommended, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_rbRecommended, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_rbRecommended, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_rbRecommended, "DialogBoxes.CastSizePlanningOptions.radioButton2");
			this.m_rbRecommended.Location = new System.Drawing.Point(6, 56);
			this.m_rbRecommended.Margin = new System.Windows.Forms.Padding(6, 6, 4, 6);
			this.m_rbRecommended.Name = "m_rbRecommended";
			this.m_rbRecommended.Size = new System.Drawing.Size(14, 13);
			this.m_rbRecommended.TabIndex = 1;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_rbRecommended, true);
			this.m_rbRecommended.UseVisualStyleBackColor = true;
			this.m_rbRecommended.CheckedChanged += new System.EventHandler(this.OptionCheckedChanged);
			// 
			// m_rbLarge
			// 
			this.m_rbLarge.AutoSize = true;
			this.glyssenColorPalette.SetBackColor(this.m_rbLarge, GlyssenColors.BackColor);
			this.m_rbLarge.BackColor = System.Drawing.SystemColors.Control;
			this.m_rbLarge.FlatAppearance.BorderColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_rbLarge, GlyssenColors.ForeColor);
			this.m_rbLarge.ForeColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetForeColor(this.m_rbLarge, GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_rbLarge, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_rbLarge, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_rbLarge, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_rbLarge, "DialogBoxes.CastSizePlanningOptions.radioButton3");
			this.m_rbLarge.Location = new System.Drawing.Point(6, 81);
			this.m_rbLarge.Margin = new System.Windows.Forms.Padding(6, 6, 4, 6);
			this.m_rbLarge.Name = "m_rbLarge";
			this.m_rbLarge.Size = new System.Drawing.Size(14, 13);
			this.m_rbLarge.TabIndex = 2;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_rbLarge, true);
			this.m_rbLarge.UseVisualStyleBackColor = true;
			this.m_rbLarge.CheckedChanged += new System.EventHandler(this.OptionCheckedChanged);
			// 
			// m_rbCustom
			// 
			this.m_rbCustom.AutoSize = true;
			this.glyssenColorPalette.SetBackColor(this.m_rbCustom, GlyssenColors.BackColor);
			this.m_rbCustom.BackColor = System.Drawing.SystemColors.Control;
			this.m_rbCustom.FlatAppearance.BorderColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_rbCustom, GlyssenColors.ForeColor);
			this.m_rbCustom.ForeColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetForeColor(this.m_rbCustom, GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_rbCustom, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_rbCustom, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_rbCustom, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_rbCustom, "DialogBoxes.CastSizePlanningOptions.radioButton4");
			this.m_rbCustom.Location = new System.Drawing.Point(6, 106);
			this.m_rbCustom.Margin = new System.Windows.Forms.Padding(6, 6, 4, 6);
			this.m_rbCustom.Name = "m_rbCustom";
			this.m_rbCustom.Size = new System.Drawing.Size(14, 13);
			this.m_rbCustom.TabIndex = 3;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_rbCustom, true);
			this.m_rbCustom.UseVisualStyleBackColor = true;
			this.m_rbCustom.CheckedChanged += new System.EventHandler(this.OptionCheckedChanged);
			// 
			// m_lblCastSize
			// 
			this.m_lblCastSize.AutoSize = true;
			this.m_lblCastSize.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblCastSize, GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_lblCastSize, GlyssenColors.ForeColor);
			this.m_lblCastSize.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblCastSize, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblCastSize, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblCastSize, "DialogBoxes.CastSizePlanningOptions.CastSize");
			this.m_lblCastSize.Location = new System.Drawing.Point(30, 6);
			this.m_lblCastSize.Margin = new System.Windows.Forms.Padding(6);
			this.m_lblCastSize.Name = "m_lblCastSize";
			this.m_lblCastSize.Size = new System.Drawing.Size(51, 13);
			this.m_lblCastSize.TabIndex = 5;
			this.m_lblCastSize.Text = "Cast Size";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblCastSize, true);
			// 
			// m_lblMen
			// 
			this.m_lblMen.AutoSize = true;
			this.m_lblMen.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblMen, GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_lblMen, GlyssenColors.ForeColor);
			this.m_lblMen.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblMen, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblMen, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblMen, "DialogBoxes.CastSizePlanningOptions.Men");
			this.m_lblMen.Location = new System.Drawing.Point(407, 6);
			this.m_lblMen.Margin = new System.Windows.Forms.Padding(6);
			this.m_lblMen.Name = "m_lblMen";
			this.m_lblMen.Size = new System.Drawing.Size(28, 13);
			this.m_lblMen.TabIndex = 6;
			this.m_lblMen.Text = "Men";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblMen, true);
			// 
			// m_lblWomen
			// 
			this.m_lblWomen.AutoSize = true;
			this.m_lblWomen.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblWomen, GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_lblWomen, GlyssenColors.ForeColor);
			this.m_lblWomen.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblWomen, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblWomen, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblWomen, "DialogBoxes.CastSizePlanningOptions.Women");
			this.m_lblWomen.Location = new System.Drawing.Point(457, 6);
			this.m_lblWomen.Margin = new System.Windows.Forms.Padding(6);
			this.m_lblWomen.Name = "m_lblWomen";
			this.m_lblWomen.Size = new System.Drawing.Size(44, 13);
			this.m_lblWomen.TabIndex = 7;
			this.m_lblWomen.Text = "Women";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblWomen, true);
			// 
			// m_lblTotal
			// 
			this.m_lblTotal.AutoSize = true;
			this.m_lblTotal.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblTotal, GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_lblTotal, GlyssenColors.ForeColor);
			this.m_lblTotal.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblTotal, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblTotal, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblTotal, "DialogBoxes.CastSizePlanningOptions.Total");
			this.m_lblTotal.Location = new System.Drawing.Point(570, 6);
			this.m_lblTotal.Margin = new System.Windows.Forms.Padding(6);
			this.m_lblTotal.Name = "m_lblTotal";
			this.m_lblTotal.Size = new System.Drawing.Size(31, 13);
			this.m_lblTotal.TabIndex = 9;
			this.m_lblTotal.Text = "Total";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblTotal, true);
			// 
			// m_lblChildren
			// 
			this.m_lblChildren.AutoSize = true;
			this.m_lblChildren.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblChildren, GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_lblChildren, GlyssenColors.ForeColor);
			this.m_lblChildren.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblChildren, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblChildren, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblChildren, "DialogBoxes.CastSizePlanningOptions.Children");
			this.m_lblChildren.Location = new System.Drawing.Point(513, 6);
			this.m_lblChildren.Margin = new System.Windows.Forms.Padding(6);
			this.m_lblChildren.Name = "m_lblChildren";
			this.m_lblChildren.Size = new System.Drawing.Size(45, 13);
			this.m_lblChildren.TabIndex = 8;
			this.m_lblChildren.Text = "Children";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblChildren, true);
			// 
			// m_lblSmall
			// 
			this.m_lblSmall.AutoSize = true;
			this.m_lblSmall.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblSmall, GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_lblSmall, GlyssenColors.ForeColor);
			this.m_lblSmall.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblSmall, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblSmall, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblSmall, "DialogBoxes.CastSizePlanningOptions.Small");
			this.m_lblSmall.Location = new System.Drawing.Point(30, 31);
			this.m_lblSmall.Margin = new System.Windows.Forms.Padding(6);
			this.m_lblSmall.Name = "m_lblSmall";
			this.m_lblSmall.Size = new System.Drawing.Size(32, 13);
			this.m_lblSmall.TabIndex = 10;
			this.m_lblSmall.Text = "Small";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblSmall, true);
			// 
			// m_lblRecommended
			// 
			this.m_lblRecommended.AutoSize = true;
			this.m_lblRecommended.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblRecommended, GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_lblRecommended, GlyssenColors.ForeColor);
			this.m_lblRecommended.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblRecommended, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblRecommended, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblRecommended, "DialogBoxes.CastSizePlanningOptions.Recommended");
			this.m_lblRecommended.Location = new System.Drawing.Point(30, 56);
			this.m_lblRecommended.Margin = new System.Windows.Forms.Padding(6);
			this.m_lblRecommended.Name = "m_lblRecommended";
			this.m_lblRecommended.Size = new System.Drawing.Size(79, 13);
			this.m_lblRecommended.TabIndex = 11;
			this.m_lblRecommended.Text = "Recommended";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblRecommended, true);
			// 
			// m_lblLarge
			// 
			this.m_lblLarge.AutoSize = true;
			this.m_lblLarge.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblLarge, GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_lblLarge, GlyssenColors.ForeColor);
			this.m_lblLarge.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblLarge, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblLarge, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblLarge, "DialogBoxes.CastSizePlanningOptions.Large");
			this.m_lblLarge.Location = new System.Drawing.Point(30, 81);
			this.m_lblLarge.Margin = new System.Windows.Forms.Padding(6);
			this.m_lblLarge.Name = "m_lblLarge";
			this.m_lblLarge.Size = new System.Drawing.Size(34, 13);
			this.m_lblLarge.TabIndex = 12;
			this.m_lblLarge.Text = "Large";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblLarge, true);
			// 
			// m_lblCustom
			// 
			this.m_lblCustom.AutoSize = true;
			this.m_lblCustom.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblCustom, GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_lblCustom, GlyssenColors.ForeColor);
			this.m_lblCustom.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblCustom, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblCustom, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblCustom, "DialogBoxes.CastSizePlanningOptions.Custom");
			this.m_lblCustom.Location = new System.Drawing.Point(30, 106);
			this.m_lblCustom.Margin = new System.Windows.Forms.Padding(6);
			this.m_lblCustom.Name = "m_lblCustom";
			this.m_lblCustom.Size = new System.Drawing.Size(42, 13);
			this.m_lblCustom.TabIndex = 13;
			this.m_lblCustom.Text = "Custom";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblCustom, true);
			// 
			// m_lblMatchList
			// 
			this.m_lblMatchList.AutoSize = true;
			this.m_lblMatchList.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblMatchList, GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_lblMatchList, GlyssenColors.ForeColor);
			this.m_lblMatchList.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblMatchList, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblMatchList, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblMatchList, "DialogBoxes.CastSizePlanningOptions.MatchList");
			this.m_lblMatchList.Location = new System.Drawing.Point(30, 132);
			this.m_lblMatchList.Margin = new System.Windows.Forms.Padding(6);
			this.m_lblMatchList.Name = "m_lblMatchList";
			this.m_lblMatchList.Size = new System.Drawing.Size(114, 13);
			this.m_lblMatchList.TabIndex = 14;
			this.m_lblMatchList.Text = "Match Voice Actor List";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblMatchList, true);
			// 
			// m_lblSmallMen
			// 
			this.m_lblSmallMen.AutoSize = true;
			this.m_lblSmallMen.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblSmallMen, GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_lblSmallMen, GlyssenColors.ForeColor);
			this.m_lblSmallMen.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblSmallMen, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblSmallMen, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_lblSmallMen, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblSmallMen, "DialogBoxes.CastSizePlanningOptions.Empty");
			this.m_lblSmallMen.Location = new System.Drawing.Point(407, 31);
			this.m_lblSmallMen.Margin = new System.Windows.Forms.Padding(6);
			this.m_lblSmallMen.Name = "m_lblSmallMen";
			this.m_lblSmallMen.Size = new System.Drawing.Size(13, 13);
			this.m_lblSmallMen.TabIndex = 15;
			this.m_lblSmallMen.Text = "0";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblSmallMen, true);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.label1, GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.label1, GlyssenColors.ForeColor);
			this.label1.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.label1, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.label1, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.label1, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.label1, "DialogBoxes.CastSizePlanningOptions.Empty");
			this.label1.Location = new System.Drawing.Point(457, 31);
			this.label1.Margin = new System.Windows.Forms.Padding(6);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(13, 13);
			this.label1.TabIndex = 16;
			this.label1.Text = "0";
			this.glyssenColorPalette.SetUsePaletteColors(this.label1, true);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.label2, GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.label2, GlyssenColors.ForeColor);
			this.label2.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.label2, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.label2, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.label2, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.label2, "DialogBoxes.CastSizePlanningOptions.Empty");
			this.label2.Location = new System.Drawing.Point(513, 31);
			this.label2.Margin = new System.Windows.Forms.Padding(6);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(13, 13);
			this.label2.TabIndex = 17;
			this.label2.Text = "0";
			this.glyssenColorPalette.SetUsePaletteColors(this.label2, true);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.label3, GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.label3, GlyssenColors.ForeColor);
			this.label3.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.label3, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.label3, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.label3, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.label3, "DialogBoxes.CastSizePlanningOptions.Empty");
			this.label3.Location = new System.Drawing.Point(570, 31);
			this.label3.Margin = new System.Windows.Forms.Padding(6);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(13, 13);
			this.label3.TabIndex = 18;
			this.label3.Text = "0";
			this.glyssenColorPalette.SetUsePaletteColors(this.label3, true);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.label4, GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.label4, GlyssenColors.ForeColor);
			this.label4.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.label4, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.label4, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.label4, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.label4, "DialogBoxes.CastSizePlanningOptions.Empty");
			this.label4.Location = new System.Drawing.Point(407, 56);
			this.label4.Margin = new System.Windows.Forms.Padding(6);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(13, 13);
			this.label4.TabIndex = 19;
			this.label4.Text = "0";
			this.glyssenColorPalette.SetUsePaletteColors(this.label4, true);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.label5, GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.label5, GlyssenColors.ForeColor);
			this.label5.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.label5, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.label5, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.label5, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.label5, "DialogBoxes.CastSizePlanningOptions.Empty");
			this.label5.Location = new System.Drawing.Point(457, 56);
			this.label5.Margin = new System.Windows.Forms.Padding(6);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(13, 13);
			this.label5.TabIndex = 20;
			this.label5.Text = "0";
			this.glyssenColorPalette.SetUsePaletteColors(this.label5, true);
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.label6, GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.label6, GlyssenColors.ForeColor);
			this.label6.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.label6, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.label6, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.label6, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.label6, "DialogBoxes.CastSizePlanningOptions.Empty");
			this.label6.Location = new System.Drawing.Point(513, 56);
			this.label6.Margin = new System.Windows.Forms.Padding(6);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(13, 13);
			this.label6.TabIndex = 21;
			this.label6.Text = "0";
			this.glyssenColorPalette.SetUsePaletteColors(this.label6, true);
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.label7, GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.label7, GlyssenColors.ForeColor);
			this.label7.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.label7, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.label7, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.label7, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.label7, "DialogBoxes.CastSizePlanningOptions.Empty");
			this.label7.Location = new System.Drawing.Point(570, 56);
			this.label7.Margin = new System.Windows.Forms.Padding(6);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(13, 13);
			this.label7.TabIndex = 22;
			this.label7.Text = "0";
			this.glyssenColorPalette.SetUsePaletteColors(this.label7, true);
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.label8, GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.label8, GlyssenColors.ForeColor);
			this.label8.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.label8, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.label8, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.label8, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.label8, "DialogBoxes.CastSizePlanningOptions.Empty");
			this.label8.Location = new System.Drawing.Point(407, 81);
			this.label8.Margin = new System.Windows.Forms.Padding(6);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(13, 13);
			this.label8.TabIndex = 23;
			this.label8.Text = "0";
			this.glyssenColorPalette.SetUsePaletteColors(this.label8, true);
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.label9, GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.label9, GlyssenColors.ForeColor);
			this.label9.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.label9, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.label9, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.label9, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.label9, "DialogBoxes.CastSizePlanningOptions.Empty");
			this.label9.Location = new System.Drawing.Point(457, 81);
			this.label9.Margin = new System.Windows.Forms.Padding(6);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(13, 13);
			this.label9.TabIndex = 24;
			this.label9.Text = "0";
			this.glyssenColorPalette.SetUsePaletteColors(this.label9, true);
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.label10, GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.label10, GlyssenColors.ForeColor);
			this.label10.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.label10, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.label10, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.label10, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.label10, "DialogBoxes.CastSizePlanningOptions.Empty");
			this.label10.Location = new System.Drawing.Point(513, 81);
			this.label10.Margin = new System.Windows.Forms.Padding(6);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(13, 13);
			this.label10.TabIndex = 25;
			this.label10.Text = "0";
			this.glyssenColorPalette.SetUsePaletteColors(this.label10, true);
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.label11, GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.label11, GlyssenColors.ForeColor);
			this.label11.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.label11, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.label11, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.label11, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.label11, "DialogBoxes.CastSizePlanningOptions.Empty");
			this.label11.Location = new System.Drawing.Point(570, 81);
			this.label11.Margin = new System.Windows.Forms.Padding(6);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(13, 13);
			this.label11.TabIndex = 26;
			this.label11.Text = "0";
			this.glyssenColorPalette.SetUsePaletteColors(this.label11, true);
			// 
			// label12
			// 
			this.label12.AutoSize = true;
			this.label12.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.label12, GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.label12, GlyssenColors.ForeColor);
			this.label12.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.label12, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.label12, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.label12, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.label12, "DialogBoxes.CastSizePlanningOptions.Empty");
			this.label12.Location = new System.Drawing.Point(570, 106);
			this.label12.Margin = new System.Windows.Forms.Padding(6);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(13, 13);
			this.label12.TabIndex = 27;
			this.label12.Text = "0";
			this.glyssenColorPalette.SetUsePaletteColors(this.label12, true);
			// 
			// label13
			// 
			this.label13.AutoSize = true;
			this.label13.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.label13, GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.label13, GlyssenColors.ForeColor);
			this.label13.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.label13, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.label13, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.label13, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.label13, "DialogBoxes.CastSizePlanningOptions.Empty");
			this.label13.Location = new System.Drawing.Point(570, 132);
			this.label13.Margin = new System.Windows.Forms.Padding(6);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(13, 13);
			this.label13.TabIndex = 28;
			this.label13.Text = "0";
			this.glyssenColorPalette.SetUsePaletteColors(this.label13, true);
			// 
			// label14
			// 
			this.label14.AutoSize = true;
			this.label14.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.label14, GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.label14, GlyssenColors.ForeColor);
			this.label14.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.label14, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.label14, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.label14, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.label14, "DialogBoxes.CastSizePlanningOptions.Empty");
			this.label14.Location = new System.Drawing.Point(513, 132);
			this.label14.Margin = new System.Windows.Forms.Padding(6);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(13, 13);
			this.label14.TabIndex = 29;
			this.label14.Text = "0";
			this.glyssenColorPalette.SetUsePaletteColors(this.label14, true);
			// 
			// label15
			// 
			this.label15.AutoSize = true;
			this.label15.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.label15, GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.label15, GlyssenColors.ForeColor);
			this.label15.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.label15, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.label15, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.label15, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.label15, "DialogBoxes.CastSizePlanningOptions.Empty");
			this.label15.Location = new System.Drawing.Point(457, 132);
			this.label15.Margin = new System.Windows.Forms.Padding(6);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(13, 13);
			this.label15.TabIndex = 30;
			this.label15.Text = "0";
			this.glyssenColorPalette.SetUsePaletteColors(this.label15, true);
			// 
			// label16
			// 
			this.label16.AutoSize = true;
			this.label16.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.label16, GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.label16, GlyssenColors.ForeColor);
			this.label16.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.label16, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.label16, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.label16, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.label16, "DialogBoxes.CastSizePlanningOptions.Empty");
			this.label16.Location = new System.Drawing.Point(407, 132);
			this.label16.Margin = new System.Windows.Forms.Padding(6);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(13, 13);
			this.label16.TabIndex = 31;
			this.label16.Text = "0";
			this.glyssenColorPalette.SetUsePaletteColors(this.label16, true);
			// 
			// numericUpDown1
			// 
			this.numericUpDown1.AutoSize = true;
			this.glyssenColorPalette.SetBackColor(this.numericUpDown1, GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.numericUpDown1, GlyssenColors.Default);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.numericUpDown1, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.numericUpDown1, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.numericUpDown1, "DialogBoxes.numericUpDown1");
			this.numericUpDown1.Location = new System.Drawing.Point(407, 103);
			this.numericUpDown1.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
			this.numericUpDown1.Name = "numericUpDown1";
			this.numericUpDown1.Size = new System.Drawing.Size(41, 20);
			this.numericUpDown1.TabIndex = 32;
			this.glyssenColorPalette.SetUsePaletteColors(this.numericUpDown1, false);
			this.numericUpDown1.ValueChanged += new System.EventHandler(this.CastSizeValueChanged);
			// 
			// numericUpDown2
			// 
			this.numericUpDown2.AutoSize = true;
			this.glyssenColorPalette.SetBackColor(this.numericUpDown2, GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.numericUpDown2, GlyssenColors.Default);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.numericUpDown2, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.numericUpDown2, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.numericUpDown2, "DialogBoxes.numericUpDown1");
			this.numericUpDown2.Location = new System.Drawing.Point(457, 103);
			this.numericUpDown2.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
			this.numericUpDown2.Name = "numericUpDown2";
			this.numericUpDown2.Size = new System.Drawing.Size(41, 20);
			this.numericUpDown2.TabIndex = 33;
			this.glyssenColorPalette.SetUsePaletteColors(this.numericUpDown2, false);
			this.numericUpDown2.ValueChanged += new System.EventHandler(this.CastSizeValueChanged);
			// 
			// numericUpDown3
			// 
			this.numericUpDown3.AutoSize = true;
			this.glyssenColorPalette.SetBackColor(this.numericUpDown3, GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.numericUpDown3, GlyssenColors.Default);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.numericUpDown3, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.numericUpDown3, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.numericUpDown3, "DialogBoxes.numericUpDown1");
			this.numericUpDown3.Location = new System.Drawing.Point(513, 103);
			this.numericUpDown3.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
			this.numericUpDown3.Name = "numericUpDown3";
			this.numericUpDown3.Size = new System.Drawing.Size(41, 20);
			this.numericUpDown3.TabIndex = 34;
			this.glyssenColorPalette.SetUsePaletteColors(this.numericUpDown3, false);
			this.numericUpDown3.ValueChanged += new System.EventHandler(this.CastSizeValueChanged);
			// 
			// m_l10NSharpExtender
			// 
			this.m_l10NSharpExtender.LocalizationManagerId = "Glyssen";
			this.m_l10NSharpExtender.PrefixForNewItems = "DialogBoxes";
			// 
			// CastSizePlanningOptions
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.glyssenColorPalette.SetBackColor(this, GlyssenColors.BackColor);
			this.Controls.Add(this.m_tableLayout);
			this.glyssenColorPalette.SetForeColor(this, GlyssenColors.ForeColor);
			this.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this, null);
			this.m_l10NSharpExtender.SetLocalizingId(this, "DialogBoxes.CastSizePlanningOptions.CastSizePlanningOptions");
			this.Margin = new System.Windows.Forms.Padding(0);
			this.Name = "CastSizePlanningOptions";
			this.Size = new System.Drawing.Size(607, 175);
			this.glyssenColorPalette.SetUsePaletteColors(this, true);
			this.m_tableLayout.ResumeLayout(false);
			this.m_tableLayout.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown3)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.glyssenColorPalette)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel m_tableLayout;
		private System.Windows.Forms.RadioButton m_rbMatchVoiceActorList;
		private System.Windows.Forms.RadioButton m_rbSmall;
		private System.Windows.Forms.RadioButton m_rbRecommended;
		private System.Windows.Forms.RadioButton m_rbLarge;
		private System.Windows.Forms.RadioButton m_rbCustom;
		private GlyssenColorPalette glyssenColorPalette;
		private System.Windows.Forms.Label m_lblCastSize;
		private L10NSharp.UI.L10NSharpExtender m_l10NSharpExtender;
		private System.Windows.Forms.Label m_lblMen;
		private System.Windows.Forms.Label m_lblWomen;
		private System.Windows.Forms.Label m_lblTotal;
		private System.Windows.Forms.Label m_lblChildren;
		private System.Windows.Forms.Label m_lblSmall;
		private System.Windows.Forms.Label m_lblRecommended;
		private System.Windows.Forms.Label m_lblLarge;
		private System.Windows.Forms.Label m_lblCustom;
		private System.Windows.Forms.Label m_lblMatchList;
		private System.Windows.Forms.Label m_lblSmallMen;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.Label label15;
		private System.Windows.Forms.Label label16;
		private System.Windows.Forms.NumericUpDown numericUpDown1;
		private System.Windows.Forms.NumericUpDown numericUpDown2;
		private System.Windows.Forms.NumericUpDown numericUpDown3;
	}
}
