using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;
using Gecko;
using Gecko.DOM;
using Gecko.Events;
using ProtoScript.Dialogs;
using SIL.Windows.Forms.PortableSettingsProvider;

namespace ProtoScript.Controls
{
	public enum ScriptBlocksViewType
	{
		Html,
		Grid,
	}

	public partial class ScriptBlocksViewer : UserControl
	{
		private const int kContextBlocksBackward = 10;
		private const int kContextBlocksForward = 10;

		private BlockNavigatorViewModel m_viewModel;
		private Func<string, string> m_getCharacterIdForUi;
		private Func<Block, string> m_getDelivery;
		private ToolTip m_toolTip;
		private ScriptBlocksViewType m_viewType;

		#region Construction and Initialization
		public ScriptBlocksViewer()
		{
			InitializeComponent();
		}

		public void Initialize(BlockNavigatorViewModel viewModel, Func<string, string> getCharacterIdForUi = null, Func<Block, string> getDelivery = null)
		{
			m_viewModel = viewModel;
			m_getCharacterIdForUi = getCharacterIdForUi;
			m_getDelivery = getDelivery;
			m_dataGridViewBlocks.Initialize(m_viewModel);

			m_viewModel.BackwardContextBlockCount = kContextBlocksBackward;
			m_viewModel.ForwardContextBlockCount = kContextBlocksForward;

			if (m_getCharacterIdForUi == null)
				m_dataGridViewBlocks.Columns.Remove(colCharacter);

			if (m_getDelivery == null)
				m_dataGridViewBlocks.Columns.Remove(colDelivery);

			m_dataGridViewBlocks.Dock = DockStyle.Fill;

			Disposed += ScriptBlocksViewer_Disposed;

			m_viewModel.CurrentBlockChanged += UpdateContextBlocksDisplay;
			m_blocksDisplayBrowser.VisibleChanged += UpdateContextBlocksDisplay;
			m_dataGridViewBlocks.VisibleChanged += UpdateContextBlocksDisplay;
		}

		void ScriptBlocksViewer_Disposed(object sender, EventArgs e)
		{
			m_viewModel.CurrentBlockChanged -= UpdateContextBlocksDisplay;
			m_blocksDisplayBrowser.VisibleChanged -= UpdateContextBlocksDisplay;
			m_dataGridViewBlocks.VisibleChanged -= UpdateContextBlocksDisplay;

			Disposed -= ScriptBlocksViewer_Disposed;
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
			UpdateContextBlocksDisplay(null, null);
		}

		public void DecreaseFont()
		{
			m_viewModel.FontSizeUiAdjustment--;
			UpdateContextBlocksDisplay(null, null);
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

		private void UpdateContextBlocksDisplay(object sender, EventArgs args)
		{
			this.SafeInvoke(() =>
			{
				if (m_blocksDisplayBrowser.Visible)
					m_blocksDisplayBrowser.DisplayHtml(m_viewModel.Html);
				else
					m_dataGridViewBlocks.UpdateContext();
			}, true);
		}

		private void HandleDataGridViewBlocksCellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
		{
			var block = m_viewModel.GetNthBlockInCurrentBook(e.RowIndex);
			if (e.ColumnIndex == colCharacter.Index)
			{
				Debug.Assert(m_getCharacterIdForUi != null);
				e.Value = m_getCharacterIdForUi(block.CharacterId);
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
			if (e.Target == null || m_getCharacterIdForUi == null || !m_blocksDisplayBrowser.Visible)
				return;

			var geckoElement = e.Target.CastToGeckoElement();
			var divElement = geckoElement as GeckoDivElement;
			if (divElement == null)
				return;

			if (divElement.Parent.ClassName == BlockNavigatorViewModel.kCssClassContext)
			{
				m_toolTip = new ToolTip {IsBalloon = true};
				// 42 and 43 are the magic numbers which happens to make these display in the correct place
				// REVIEW: it would be nice to figure out a better way to place these which is more robust. These numbers have changed several times already
				int x = m_blocksDisplayBrowser.Location.X + m_blocksDisplayBrowser.Size.Width - 42;
				int y = m_blocksDisplayBrowser.Location.Y + e.ClientY - m_blocksDisplayBrowser.Margin.Top - 43;
				m_toolTip.Show(m_getCharacterIdForUi(divElement.Parent.GetAttribute(BlockNavigatorViewModel.kDataCharacter)), this,
					x, y);
			}
		}

		private void OnMouseOut(object sender, DomMouseEventArgs e)
		{
			if (e.Target == null || m_toolTip == null)
				return;
			var geckoElement = e.Target.CastToGeckoElement();
			var divElement = geckoElement as GeckoDivElement;
			if (divElement == null)
				return;

			if (divElement.Parent.ClassName == BlockNavigatorViewModel.kCssClassContext)
			{
				m_toolTip.Hide(this);
				m_toolTip.Dispose();
				m_toolTip = null;
			}
		}

		private void OnDocumentCompleted(object sender, GeckoDocumentCompletedEventArgs e)
		{
			m_blocksDisplayBrowser.ScrollElementIntoView(BlockNavigatorViewModel.kMainQuoteElementId, -225);
		}
		#endregion
	}
}
