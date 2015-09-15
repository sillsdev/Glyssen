using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Glyssen.Character;

namespace Glyssen.Controls
{
	public partial class VoiceActorInformationGridReadonly : UserControl
	{
		public event DataGridViewCellMouseEventHandler CellDoubleClicked;
		public event MouseEventHandler GridMouseMove;
		public event EventHandler SelectionChanged;
		private readonly VoiceActorInformationViewModel m_actorInformationViewModel;
		private readonly Font m_italicsFont;

		public VoiceActorInformationGridReadonly()
		{
			InitializeComponent();

			m_actorInformationViewModel = new VoiceActorInformationViewModel();

			m_dataGrid.DataError += m_dataGrid_DataError;

			//Ensures that rows stay the height we set in the designer (specifically to match the character groups grid)
			m_dataGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

			m_dataGrid.ReadOnly = true;

			m_dataGrid.CellMouseDoubleClick += HandleDoubleClick;
			m_dataGrid.MouseMove += HandleMouseMove;
			m_dataGrid.CellFormatting += HandleCellFormatting;

			Font originalGridFont = m_dataGrid.Font;
			m_italicsFont = new Font(originalGridFont.FontFamily, originalGridFont.Size, originalGridFont.Style | FontStyle.Italic);
		}

		void m_dataGrid_DataError(object sender, DataGridViewDataErrorEventArgs e)
		{
			throw e.Exception;
		}

		public int RowCount { get { return m_dataGrid.RowCount; } }

		public VoiceActor.VoiceActor SelectedVoiceActorEntity
		{
			get 
			{
				if (m_dataGrid.SelectedRows.Count == 0)
					return null;
				return m_dataGrid.SelectedRows[0].DataBoundItem as VoiceActor.VoiceActor; 
			}
		}

		public DataGridViewSelectedRowCollection SelectedRows
		{
			get { return m_dataGrid.SelectedRows; }
		}

		public override ContextMenuStrip ContextMenuStrip
		{
			get { return m_dataGrid.ContextMenuStrip; }
			set { m_dataGrid.ContextMenuStrip = value; }
		}

		public IEnumerable<CharacterGroup> CharacterGroupsWithAssignedActors
		{
			get { return m_actorInformationViewModel.CharacterGroupsWithAssignedActors; }
			set { m_actorInformationViewModel.CharacterGroupsWithAssignedActors = value; }
		}

		public void Initialize(Project project, bool sort = true)
		{
			m_actorInformationViewModel.Initialize(project);

			m_dataGrid.DataSource = m_actorInformationViewModel.BindingList;

			if (sort)
				m_dataGrid.Sort(m_dataGrid.Columns["ActorName"], ListSortDirection.Ascending);
		}

		public DataGridView.HitTestInfo HitTest(int x, int y)
		{
			return m_dataGrid.HitTest(x, y);
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			if (ParentForm != null)
				ParentForm.Closing += ParentForm_Closing;
		}

		void ParentForm_Closing(object sender, CancelEventArgs e)
		{
			e.Cancel = !ValidateChildren();
		}

		public void RefreshSort()
		{
			if (m_actorInformationViewModel.ActorCount > 0)
				m_dataGrid.Sort(m_dataGrid.SortedColumn, m_dataGrid.SortOrder == SortOrder.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending);
			else
			{
				//This is obviously a hack, but I can't get it to refresh the grid when the last actor is removed
				m_dataGrid.DataSource = new List<VoiceActor.VoiceActor>();
				m_dataGrid.DataSource = m_actorInformationViewModel.BindingList;
			}
		}

		public Color BackgroundColor
		{
			get { return m_dataGrid.BackgroundColor; }
			set { m_dataGrid.BackgroundColor = value; }
		}

		private void HandleDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
		{
			DataGridViewCellMouseEventHandler handler = CellDoubleClicked;
			if (handler != null)
				handler(sender, e);
		}

		private void HandleMouseMove(object sender, MouseEventArgs e)
		{
			MouseEventHandler handler = GridMouseMove;
			if (handler != null)
				handler(sender, e);
		}

		private void HandleCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
		{
			VoiceActor.VoiceActor actor = m_dataGrid.Rows[e.RowIndex].DataBoundItem as VoiceActor.VoiceActor;
			if (actor == null || CharacterGroupsWithAssignedActors == null || !CharacterGroupsWithAssignedActors.Any())
			{
				e.FormattingApplied = false;
				return;
			}
			if (CharacterGroupsWithAssignedActors.Any(cg => cg.VoiceActorAssigned == actor))
			{
				e.CellStyle.Font = m_italicsFont;
				e.CellStyle.ForeColor = Color.Gray;
			}
			else
			{
				e.FormattingApplied = false;
			}
		}

		private void m_dataGrid_SelectionChanged(object sender, EventArgs e)
		{
			EventHandler handler = SelectionChanged;
			if (handler != null)
				handler(sender, e);
		}
	}
}
