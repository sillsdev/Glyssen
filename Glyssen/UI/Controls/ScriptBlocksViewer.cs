using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Gecko;
using Gecko.DOM;
using Gecko.Events;
using Glyssen.Utilities;
using L10NSharp;
using SIL.Windows.Forms.Extensions;
using SIL.Windows.Forms.PortableSettingsProvider;
using Waxuquerque;
using Waxuquerque.ViewModel;

namespace Glyssen.UI.Controls
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

		public event EventHandler SelectionChanged;
		public event EventHandler MinimumWidthChanged;

		#region Construction and Initialization
		public ScriptBlocksViewer()
		{
			InitializeComponent();

			DataGridViewBlocksOnMinimumWidthChanged(m_dataGridViewBlocks, new EventArgs());
			m_dataGridViewBlocks.MinimumWidthChanged += DataGridViewBlocksOnMinimumWidthChanged;
		}

		private void DataGridViewBlocksOnMinimumWidthChanged(object sender, EventArgs eventArgs)
		{
			var minWidth = Math.Max(m_blocksDisplayBrowser.MinimumSize.Width, m_dataGridViewBlocks.MinimumSize.Width) + Padding.Horizontal;

			if (minWidth != MinimumSize.Width)
			{
				MinimumSize = new Size(minWidth,
					Math.Max(m_blocksDisplayBrowser.MinimumSize.Height, m_dataGridViewBlocks.MinimumSize.Height) + Padding.Vertical);
				if (MinimumWidthChanged != null)
					MinimumWidthChanged(this, new EventArgs());
			}
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
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);

			m_viewModel.CurrentBlockChanged += UpdateContextBlocksDisplay;
			m_viewModel.CurrentBlockMatchupChanged += UpdateContextBlocksDisplay;
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
			set { m_title.Text = value; }
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		public BorderStyle ContentBorderStyle
		{
			get { return m_dataGridViewBlocks.BorderStyle; }
			set { m_dataGridViewBlocks.BorderStyle = m_blocksDisplayBrowser.BorderStyle = value; }
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

		public void ShowNothingMatchesFilterMessage()
		{
			string msg = LocalizationManager.GetString("DialogBoxes.ScriptBlocksViewer.NoMatches", "Nothing matches the current filter.");
			MessageBox.Show(this, msg, ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
		}
		#endregion

		#region private methods
		private void ChangeViewType(Control viewBeingShown, Control viewBeingHidden)
		{
			viewBeingHidden.Visible = false;
			viewBeingShown.Visible = true;
		}

		private void HandleSelectionChanged(object sender, EventArgs args)
		{
			UpdateContextBlocksDisplay(sender, args);
			if (SelectionChanged != null)
				SelectionChanged(this, new EventArgs());
		}

		private void UpdateContextBlocksDisplay(object sender, EventArgs args)
		{
			this.SafeInvoke(() =>
			{
				if (m_blocksDisplayBrowser.Visible)
					m_blocksDisplayBrowser.DisplayHtml(m_viewModel.Html);
				else if (m_dataGridViewBlocks.Visible)
					m_dataGridViewBlocks.UpdateContext();
			}, GetType().FullName + ".UpdateContextBlocksDisplay", SIL.Windows.Forms.Extensions.ControlExtensions.ErrorHandlingAction.IgnoreIfDisposed);
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

		private void HideToolTip()
		{
			if (m_toolTip != null)
			{
				m_toolTip.Hide(this);
				m_toolTip.Dispose();
				m_toolTip = null;
			}
		}
		#endregion

		#region Browser events
		private void OnMouseOver(object sender, DomMouseEventArgs e)
		{
			if (e.Target == null || m_getCharacterIdForUi == null || !m_blocksDisplayBrowser.Visible)
			{
				HideToolTip();
				return;
			}

			var geckoElement = e.Target.CastToGeckoElement();
			var checkElement = geckoElement as GeckoHtmlElement;

			for (int i = 0; i < 10; i++)
			{
				if (checkElement == null)
				{
					HideToolTip();
					return;
				}
				else if (checkElement.ClassName != BlockNavigatorViewModel.kCssClassContext)
					checkElement = checkElement.Parent;
				else
					break;
			}

			if (m_toolTip == null)
			{
				m_toolTip = new ToolTip { IsBalloon = true };
				string toolTipText = m_getCharacterIdForUi(checkElement.GetAttribute(BlockNavigatorViewModel.kDataCharacter));

				// 42 and 43 are the magic numbers which happens to make these display in the correct place
				// REVIEW: it would be nice to figure out a better way to place these which is more robust. These numbers have changed several times already
				int x = m_blocksDisplayBrowser.Location.X + m_blocksDisplayBrowser.Size.Width - 42;
				int y = m_blocksDisplayBrowser.Location.Y + e.ClientY - m_blocksDisplayBrowser.Margin.Top - 43;

				m_toolTip.Show(toolTipText, this, x, y);
			}
		}

		private void OnDocumentCompleted(object sender, GeckoDocumentCompletedEventArgs e)
		{
			m_blocksDisplayBrowser.ScrollElementIntoView(BlockNavigatorViewModel.kMainQuoteElementId, -225);
		}

		private void OnMouseClick(object sender, DomMouseEventArgs e)
		{
			if (!m_blocksDisplayBrowser.Window.Selection.IsCollapsed)
				return;

			GeckoElement geckoElement;
			if (GeckoUtilities.ParseDomEventTargetAsGeckoElement(e.Target, out geckoElement))
			{
				var geckoDivElement = geckoElement as GeckoDivElement;

				while (geckoDivElement != null && !geckoDivElement.ClassName.Contains("block scripture"))
					geckoDivElement = geckoDivElement.Parent as GeckoDivElement;
				if (geckoDivElement == null)
					return;

				int blockIndexInBook;
				GeckoNode blockIndexInBookAttr = geckoDivElement.Attributes["data-block-index-in-book"];
				if (blockIndexInBookAttr == null || !Int32.TryParse(blockIndexInBookAttr.NodeValue, out blockIndexInBook))
					return;
				m_viewModel.CurrentBlockIndexInBook = blockIndexInBook;
			}
		}
		#endregion
	}
}
