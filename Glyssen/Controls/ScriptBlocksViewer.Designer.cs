using L10NSharp.UI;

namespace Glyssen.Controls
{
	partial class ScriptBlocksViewer
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
				m_dataGridViewBlocks.MinimumWidthChanged -= DataGridViewBlocksOnMinimumWidthChanged;
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
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
			this.m_dataGridViewBlocks = new Glyssen.Controls.ScriptBlocksGridView();
			this.m_blocksDisplayBrowser = new Glyssen.Controls.Browser();
			this.m_title = new System.Windows.Forms.Label();
			this.m_l10NSharpExtender = new L10NSharp.UI.L10NSharpExtender(this.components);
			this.colReference = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colCharacter = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colDelivery = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colText = new System.Windows.Forms.DataGridViewTextBoxColumn();
			((System.ComponentModel.ISupportInitialize)(this.m_dataGridViewBlocks)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).BeginInit();
			this.SuspendLayout();
			// 
			// m_dataGridViewBlocks
			// 
			this.m_dataGridViewBlocks.AllowUserToAddRows = false;
			this.m_dataGridViewBlocks.AllowUserToDeleteRows = false;
			this.m_dataGridViewBlocks.AllowUserToOrderColumns = true;
			this.m_dataGridViewBlocks.AllowUserToResizeRows = false;
			this.m_dataGridViewBlocks.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCells;
			this.m_dataGridViewBlocks.BackgroundColor = System.Drawing.SystemColors.Window;
			this.m_dataGridViewBlocks.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.m_dataGridViewBlocks.CausesValidation = false;
			dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.m_dataGridViewBlocks.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
			this.m_dataGridViewBlocks.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.m_dataGridViewBlocks.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colReference,
            this.colCharacter,
            this.colDelivery,
            this.colText});
			dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
			dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
			dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
			dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.m_dataGridViewBlocks.DefaultCellStyle = dataGridViewCellStyle3;
			this.m_dataGridViewBlocks.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_dataGridViewBlocks, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_dataGridViewBlocks, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_dataGridViewBlocks, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_dataGridViewBlocks, "ScripBlocksViewer.ScriptBlocksViewer.m_dataGridViewBlocks");
			this.m_dataGridViewBlocks.Location = new System.Drawing.Point(3, 184);
			this.m_dataGridViewBlocks.MinimumSize = new System.Drawing.Size(367, 9);
			this.m_dataGridViewBlocks.MultiSelect = false;
			this.m_dataGridViewBlocks.Name = "m_dataGridViewBlocks";
			this.m_dataGridViewBlocks.ReadOnly = true;
			this.m_dataGridViewBlocks.RowHeadersVisible = false;
			this.m_dataGridViewBlocks.RowTemplate.DefaultCellStyle.BackColor = System.Drawing.SystemColors.Window;
			this.m_dataGridViewBlocks.RowTemplate.DefaultCellStyle.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_dataGridViewBlocks.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.m_dataGridViewBlocks.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.m_dataGridViewBlocks.ShowCellErrors = false;
			this.m_dataGridViewBlocks.ShowEditingIcon = false;
			this.m_dataGridViewBlocks.ShowRowErrors = false;
			this.m_dataGridViewBlocks.Size = new System.Drawing.Size(367, 166);
			this.m_dataGridViewBlocks.StandardTab = true;
			this.m_dataGridViewBlocks.TabIndex = 2;
			this.m_dataGridViewBlocks.VirtualMode = true;
			this.m_dataGridViewBlocks.Visible = false;
			this.m_dataGridViewBlocks.CellValueNeeded += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.HandleDataGridViewBlocksCellValueNeeded);
			this.m_dataGridViewBlocks.SelectionChanged += new System.EventHandler(this.HandleSelectionChanged);
			// 
			// m_blocksDisplayBrowser
			// 
			this.m_blocksDisplayBrowser.AutoSize = true;
			this.m_blocksDisplayBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_blocksDisplayBrowser, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_blocksDisplayBrowser, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_blocksDisplayBrowser, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_blocksDisplayBrowser, "ScriptBlocksViewer.Browser");
			this.m_blocksDisplayBrowser.Location = new System.Drawing.Point(0, 0);
			this.m_blocksDisplayBrowser.Name = "m_blocksDisplayBrowser";
			this.m_blocksDisplayBrowser.Size = new System.Drawing.Size(250, 350);
			this.m_blocksDisplayBrowser.TabIndex = 0;
			this.m_blocksDisplayBrowser.OnMouseOver += new System.EventHandler<Gecko.DomMouseEventArgs>(this.OnMouseOver);
			this.m_blocksDisplayBrowser.OnMouseClick += new System.EventHandler<Gecko.DomMouseEventArgs>(this.OnMouseClick);
			this.m_blocksDisplayBrowser.OnDocumentCompleted += new System.EventHandler<Gecko.Events.GeckoDocumentCompletedEventArgs>(this.OnDocumentCompleted);
			// 
			// m_title
			// 
			this.m_title.Dock = System.Windows.Forms.DockStyle.Top;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_title, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_title, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_title, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_title, "DialogBoxes.ScriptBlocksViewer.Title");
			this.m_title.Location = new System.Drawing.Point(0, 0);
			this.m_title.Name = "m_title";
			this.m_title.Size = new System.Drawing.Size(250, 24);
			this.m_title.TabIndex = 1;
			this.m_title.Text = "Title";
			// 
			// m_l10NSharpExtender
			// 
			this.m_l10NSharpExtender.LocalizationManagerId = "Glyssen";
			this.m_l10NSharpExtender.PrefixForNewItems = "DialogBoxes.ScriptBlocksViewer";
			// 
			// colReference
			// 
			this.colReference.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
			dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.colReference.DefaultCellStyle = dataGridViewCellStyle2;
			this.colReference.HeaderText = "_L10N_:DialogBoxes.ScriptBlocksViewer.Reference!Reference";
			this.colReference.MinimumWidth = 30;
			this.colReference.Name = "colReference";
			this.colReference.ReadOnly = true;
			this.colReference.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			this.colReference.Width = 313;
			// 
			// colCharacter
			// 
			this.colCharacter.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.colCharacter.FillWeight = 200F;
			this.colCharacter.HeaderText = "_L10N_:DialogBoxes.ScriptBlocksViewer.Character!Character";
			this.colCharacter.MinimumWidth = 60;
			this.colCharacter.Name = "colCharacter";
			this.colCharacter.ReadOnly = true;
			this.colCharacter.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			// 
			// colDelivery
			// 
			this.colDelivery.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.colDelivery.HeaderText = "_L10N_:DialogBoxes.ScriptBlocksViewer.Delivery!Delivery";
			this.colDelivery.MinimumWidth = 60;
			this.colDelivery.Name = "colDelivery";
			this.colDelivery.ReadOnly = true;
			this.colDelivery.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			// 
			// colText
			// 
			this.colText.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.colText.FillWeight = 300F;
			this.colText.HeaderText = "_L10N_:DialogBoxes.ScriptBlocksViewer.Text!VernacularText";
			this.colText.MinimumWidth = 200;
			this.colText.Name = "colText";
			this.colText.ReadOnly = true;
			this.colText.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			// 
			// ScriptBlocksViewer
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Transparent;
			this.Controls.Add(this.m_dataGridViewBlocks);
			this.Controls.Add(this.m_title);
			this.Controls.Add(this.m_blocksDisplayBrowser);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this, "ScripBlocksViewer.ScriptBlocksViewer.ScriptBlocksViewer");
			this.Name = "ScriptBlocksViewer";
			this.Size = new System.Drawing.Size(250, 350);
			((System.ComponentModel.ISupportInitialize)(this.m_dataGridViewBlocks)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Browser m_blocksDisplayBrowser;
		private System.Windows.Forms.Label m_title;
		private ScriptBlocksGridView m_dataGridViewBlocks;
		private L10NSharpExtender m_l10NSharpExtender;
		private System.Windows.Forms.DataGridViewTextBoxColumn colReference;
		private System.Windows.Forms.DataGridViewTextBoxColumn colCharacter;
		private System.Windows.Forms.DataGridViewTextBoxColumn colDelivery;
		private System.Windows.Forms.DataGridViewTextBoxColumn colText;
	}
}
