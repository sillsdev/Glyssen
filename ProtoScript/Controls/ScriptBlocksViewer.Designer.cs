namespace ProtoScript.Controls
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
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
			this.m_panel = new System.Windows.Forms.Panel();
			this.m_dataGridViewBlocks = new ProtoScript.Controls.ScriptBlocksGridView();
			this.colReference = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colCharacter = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colDelivery = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colText = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.m_title = new System.Windows.Forms.Label();
			this.m_blocksDisplayBrowser = new ProtoScript.Controls.Browser();
			this.m_panel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_dataGridViewBlocks)).BeginInit();
			this.SuspendLayout();
			// 
			// m_panel
			// 
			this.m_panel.BackColor = System.Drawing.Color.Transparent;
			this.m_panel.Controls.Add(this.m_dataGridViewBlocks);
			this.m_panel.Controls.Add(this.m_blocksDisplayBrowser);
			this.m_panel.Controls.Add(this.m_title);
			this.m_panel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_panel.Location = new System.Drawing.Point(0, 0);
			this.m_panel.Name = "m_panel";
			this.m_panel.Size = new System.Drawing.Size(250, 350);
			this.m_panel.TabIndex = 0;
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
			dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
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
			dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
			dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.m_dataGridViewBlocks.DefaultCellStyle = dataGridViewCellStyle3;
			this.m_dataGridViewBlocks.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
			this.m_dataGridViewBlocks.Location = new System.Drawing.Point(0, 170);
			this.m_dataGridViewBlocks.MultiSelect = false;
			this.m_dataGridViewBlocks.Name = "m_dataGridViewBlocks";
			this.m_dataGridViewBlocks.ReadOnly = true;
			this.m_dataGridViewBlocks.RowHeadersVisible = false;
			this.m_dataGridViewBlocks.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.m_dataGridViewBlocks.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.m_dataGridViewBlocks.ShowCellErrors = false;
			this.m_dataGridViewBlocks.ShowEditingIcon = false;
			this.m_dataGridViewBlocks.ShowRowErrors = false;
			this.m_dataGridViewBlocks.Size = new System.Drawing.Size(247, 177);
			this.m_dataGridViewBlocks.TabIndex = 2;
			this.m_dataGridViewBlocks.VirtualMode = true;
			this.m_dataGridViewBlocks.Visible = false;
			this.m_dataGridViewBlocks.CellValueNeeded += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.HandleDataGridViewBlocksCellValueNeeded);
			this.m_dataGridViewBlocks.SelectionChanged += new System.EventHandler(this.HandleSelectedBlockChanged);
			// 
			// colReference
			// 
			this.colReference.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.colReference.DefaultCellStyle = dataGridViewCellStyle2;
			this.colReference.HeaderText = "Reference";
			this.colReference.MaxInputLength = 11;
			this.colReference.MinimumWidth = 30;
			this.colReference.Name = "colReference";
			this.colReference.ReadOnly = true;
			this.colReference.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			this.colReference.Width = 82;
			// 
			// colCharacter
			// 
			this.colCharacter.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.colCharacter.FillWeight = 200F;
			this.colCharacter.HeaderText = "Character";
			this.colCharacter.MinimumWidth = 60;
			this.colCharacter.Name = "colCharacter";
			this.colCharacter.ReadOnly = true;
			this.colCharacter.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			// 
			// colDelivery
			// 
			this.colDelivery.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.colDelivery.HeaderText = "Delivery";
			this.colDelivery.MinimumWidth = 60;
			this.colDelivery.Name = "colDelivery";
			this.colDelivery.ReadOnly = true;
			this.colDelivery.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			// 
			// colText
			// 
			this.colText.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.colText.FillWeight = 200F;
			this.colText.HeaderText = "Text";
			this.colText.MinimumWidth = 60;
			this.colText.Name = "colText";
			this.colText.ReadOnly = true;
			this.colText.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			// 
			// m_title
			// 
			this.m_title.Dock = System.Windows.Forms.DockStyle.Top;
			this.m_title.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_title.ForeColor = System.Drawing.Color.White;
			this.m_title.Location = new System.Drawing.Point(0, 0);
			this.m_title.Name = "m_title";
			this.m_title.Size = new System.Drawing.Size(250, 24);
			this.m_title.TabIndex = 1;
			this.m_title.Text = "Title";
			// 
			// m_blocksDisplayBrowser
			// 
			this.m_blocksDisplayBrowser.AutoSize = true;
			this.m_blocksDisplayBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_blocksDisplayBrowser.Location = new System.Drawing.Point(0, 24);
			this.m_blocksDisplayBrowser.Name = "m_blocksDisplayBrowser";
			this.m_blocksDisplayBrowser.Size = new System.Drawing.Size(250, 326);
			this.m_blocksDisplayBrowser.TabIndex = 0;
			this.m_blocksDisplayBrowser.OnMouseOver += new System.EventHandler<Gecko.DomMouseEventArgs>(this.OnMouseOver);
			this.m_blocksDisplayBrowser.OnMouseOut += new System.EventHandler<Gecko.DomMouseEventArgs>(this.OnMouseOut);
			this.m_blocksDisplayBrowser.OnDocumentCompleted += new System.EventHandler<Gecko.Events.GeckoDocumentCompletedEventArgs>(this.OnDocumentCompleted);
			// 
			// ScriptBlocksViewer
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Transparent;
			this.Controls.Add(this.m_panel);
			this.Name = "ScriptBlocksViewer";
			this.Size = new System.Drawing.Size(250, 350);
			this.m_panel.ResumeLayout(false);
			this.m_panel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_dataGridViewBlocks)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel m_panel;
		private Browser m_blocksDisplayBrowser;
		private System.Windows.Forms.Label m_title;
		private ScriptBlocksGridView m_dataGridViewBlocks;
		private System.Windows.Forms.DataGridViewTextBoxColumn colReference;
		private System.Windows.Forms.DataGridViewTextBoxColumn colCharacter;
		private System.Windows.Forms.DataGridViewTextBoxColumn colDelivery;
		private System.Windows.Forms.DataGridViewTextBoxColumn colText;
	}
}
