using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;
using Gecko;
using Gecko.DOM;
using Gecko.Events;
using Palaso.UI.WindowsForms.PortableSettingsProvider;
using ProtoScript.Dialogs;

namespace ProtoScript.Controls
{
	public enum ScriptBlocksViewType
	{
		Html,
		Grid,
	}

	public partial class ScriptBlocksViewer : UserControl
	{
		private BlockNavigatorViewModel m_viewModel;
		private Func<Block, string> m_getCharacterIdForUi;
		private Func<Block, string> m_getDelivery;
		private readonly ToolTip m_toolTip = new ToolTip { IsBalloon = true, Active = false};
		private ScriptBlocksViewType m_viewType;

		#region Construction and Initialization
		public ScriptBlocksViewer()
		{
			InitializeComponent();
		}

		public void Initialize(BlockNavigatorViewModel viewModel, Func<Block, string> getCharacterIdForUi = null, Func<Block, string> getDelivery = null)
		{
			m_viewModel = viewModel;
			m_getCharacterIdForUi = getCharacterIdForUi;
			m_getDelivery = getDelivery;
			m_dataGridViewBlocks.Initialize(m_viewModel);

			if (m_getCharacterIdForUi == null)
				m_dataGridViewBlocks.Columns.Remove(colCharacter);

			if (m_getDelivery == null)
				m_dataGridViewBlocks.Columns.Remove(colDelivery);

			m_dataGridViewBlocks.Dock = DockStyle.Fill;

			m_viewModel.CurrentBlockChanged += (sender, args) => BeginInvoke(new Action(UpdateContextBlocksDisplay));

			m_blocksDisplayBrowser.VisibleChanged += HandleSelectedBlockChanged;
			m_dataGridViewBlocks.VisibleChanged += (sender, args) => BeginInvoke(new Action(() => HandleSelectedBlockChanged(sender, args)));
		}
		#endregion

		#region public properties
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public ScriptBlocksViewType ViewType
		{
			get { return m_viewType; }
			set
			{
				m_viewType = value;

				if (m_viewType == ScriptBlocksViewType.Html)
					ChangeViewType(m_blocksDisplayBrowser, m_dataGridViewBlocks);
				else
					ChangeViewType(m_dataGridViewBlocks, m_blocksDisplayBrowser);
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public GridSettings BlocksGridSettings
		{
			get { return GridSettings.Create(m_dataGridViewBlocks); }
			set
			{
				if (value != null)
					value.InitializeGrid(m_dataGridViewBlocks);
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		public override string Text
		{
			get { return m_title.Text; }
			set { m_title.Text = value;  }
		}
		#endregion

		#region public methods
		public void IncreaseFont()
		{
			m_viewModel.FontSizeUiAdjustment++;
			UpdateContextBlocksDisplay();
		}

		public void DecreaseFont()
		{
			m_viewModel.FontSizeUiAdjustment--;
			UpdateContextBlocksDisplay();
		}

		public void Clear()
		{
			if (m_blocksDisplayBrowser.Visible)
				m_blocksDisplayBrowser.DisplayHtml(String.Empty);
			else
				m_dataGridViewBlocks.Clear();
		}
		#endregion

		#region private methods
		private void ChangeViewType(Control viewBeingShown, Control viewBeingHidden)
		{
			viewBeingShown.Visible = true;
			viewBeingHidden.Visible = false;
		}

		private void UpdateContextBlocksDisplay()
		{
			if (m_blocksDisplayBrowser.Visible)
				m_blocksDisplayBrowser.DisplayHtml(m_viewModel.Html);
			else
				m_dataGridViewBlocks.UpdateContext();
		}
		
		private void HandleSelectedBlockChanged(object sender, EventArgs e)
		{
			UpdateContextBlocksDisplay();
		}

		private void HandleDataGridViewBlocksCellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
		{
			var block = m_viewModel.GetNthBlockInCurrentBook(e.RowIndex);
			if (e.ColumnIndex == colCharacter.Index)
			{
				Debug.Assert(m_getCharacterIdForUi != null);
				e.Value = m_getCharacterIdForUi(block);
			}
			else if (e.ColumnIndex == colDelivery.Index)
			{
				Debug.Assert(m_getDelivery != null);
				e.Value = m_getDelivery(block);
			}
		}
		#endregion

		#region Browser events
		private void OnMouseOver(object sender, DomMouseEventArgs e)
		{
			if (e.Target == null)
				return;
			if (m_blocksDisplayBrowser.Visible)
			{
				var geckoElement = e.Target.CastToGeckoElement();
				var divElement = geckoElement as GeckoDivElement;
				if (divElement == null)
					return;

				if (divElement.Parent.ClassName == BlockNavigatorViewModel.kCssClassContext)
				{
					// 22 is the magic number which happens to make these display in the correct place
					int x = m_blocksDisplayBrowser.Location.X + m_blocksDisplayBrowser.Size.Width - 22;
					int y = m_blocksDisplayBrowser.Location.Y + e.ClientY - m_blocksDisplayBrowser.Margin.Top;
					m_toolTip.Show(divElement.Parent.GetAttribute(AssignCharacterViewModel.kDataCharacter), this, x, y);
				}
			}
		}

		private void OnMouseOut(object sender, DomMouseEventArgs e)
		{
			if (e.Target == null)
				return;
			var geckoElement = e.Target.CastToGeckoElement();
			var divElement = geckoElement as GeckoDivElement;
			if (divElement == null)
				return;

			if (divElement.Parent.ClassName == BlockNavigatorViewModel.kCssClassContext)
			{
				m_toolTip.Hide(this);
			}
		}

		private void OnDocumentCompleted(object sender, GeckoDocumentCompletedEventArgs e)
		{
			m_blocksDisplayBrowser.ScrollElementIntoView(BlockNavigatorViewModel.kMainQuoteElementId, -225);
		}
		#endregion
	}
}
