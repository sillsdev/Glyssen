using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Glyssen.Character;
using L10NSharp;
using SIL.Reporting;

namespace Glyssen.Controls
{
	public partial class VoiceActorInformationGrid : UserControl
	{
		public event EventHandler Saved;
		public event DataGridViewCellEventHandler CellUpdated;
		public event DataGridViewRowEventHandler UserAddedRow;
		public event DataGridViewRowsRemovedEventHandler UserRemovedRows;
		public event DataGridViewCellMouseEventHandler CellDoubleClicked;
		public event MouseEventHandler GridMouseMove;
		public event EventHandler SelectionChanged;
		private readonly VoiceActorInformationViewModel m_actorInformationViewModel;
		private readonly Font m_italicsFont;
		private bool m_inEndEdit;

		public VoiceActorInformationGrid()
		{
			InitializeComponent();

			m_actorInformationViewModel = new VoiceActorInformationViewModel();

			m_dataGrid.DataError += m_dataGrid_DataError;

			ActorGender.DataSource = m_actorInformationViewModel.GetGenderDataTable();
			ActorGender.ValueMember = "ID";
			ActorGender.DisplayMember = "Name";

			ActorAge.DataSource = m_actorInformationViewModel.GetAgeDataTable();
			ActorAge.ValueMember = "ID";
			ActorAge.DisplayMember = "Name";

			ActorQuality.DataSource = m_actorInformationViewModel.GetVoiceQualityDataTable();
			ActorQuality.ValueMember = "ID";
			ActorQuality.DisplayMember = "Name";

			m_actorInformationViewModel.Saved += m_actorInformationViewModel_Saved;

			//Ensures that rows stay the height we set in the designer (specifically to match the character groups grid)
			m_dataGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

			m_dataGrid.UserAddedRow += HandleUserAddedRow;
			m_dataGrid.CellMouseDoubleClick += HandleDoubleClick;
			m_dataGrid.MouseMove += HandleMouseMove;
			m_dataGrid.CellFormatting += HandleCellFormatting;

			Font originalGridFont = m_dataGrid.Font;
			m_italicsFont = new Font(originalGridFont.FontFamily, originalGridFont.Size, originalGridFont.Style | FontStyle.Italic);
		}

		void m_dataGrid_DataError(object sender, DataGridViewDataErrorEventArgs e)
		{
			ErrorReport.ReportFatalException(e.Exception);
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

		public bool ReadOnly
		{
			get { return m_dataGrid.ReadOnly; }
			set { m_dataGrid.ReadOnly = value; }
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

		public void SaveVoiceActorInformation()
		{
			m_actorInformationViewModel.SaveVoiceActorInformation();
		}

		public DataGridView.HitTestInfo HitTest(int x, int y)
		{
			return m_dataGrid.HitTest(x, y);
		}

		public DataGridViewEditMode EditMode
		{
			get { return m_dataGrid.EditMode; }
			set { m_dataGrid.EditMode = value; }
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

		public bool EndEdit()
		{
			m_inEndEdit = true;
			var result = ValidateChildren() && m_dataGrid.EndEdit();
			m_inEndEdit = false;
			return result;
		}

		private void m_dataGrid_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
		{
			var currentComboBox = e.Control as ComboBox;
			if (currentComboBox != null)
			{
				currentComboBox.KeyPress -= Combobox_KeyPress;
				currentComboBox.KeyPress += Combobox_KeyPress;

				if (currentComboBox.SelectedIndex == -1)
					currentComboBox.SelectedIndex = 0;
			}
		}

		private void Combobox_KeyPress(object sender, KeyPressEventArgs e)
		{
			Debug.Assert(sender is ComboBox);
			if (char.IsLetter(e.KeyChar))
			{
				m_dataGrid.MoveToNextField();
			}
		}

		private void RemoveSelectedRows(bool confirmWithUser)
		{
			if (m_dataGrid.SelectedRows.Count == 0)
				return;

			var actorsToRemove = new HashSet<VoiceActor.VoiceActor>();
			foreach (DataGridViewRow row in m_dataGrid.SelectedRows)
				actorsToRemove.Add(row.DataBoundItem as VoiceActor.VoiceActor);

			int indexOfFirstRowToRemove = m_dataGrid.SelectedRows[0].Index;
			if (m_actorInformationViewModel.DeleteVoiceActors(actorsToRemove, confirmWithUser))
			{
				DataGridViewRowsRemovedEventHandler handler = UserRemovedRows;
				if (handler != null)
					handler(m_dataGrid, new DataGridViewRowsRemovedEventArgs(indexOfFirstRowToRemove, m_dataGrid.RowCount));				
			}
		}

		public void RefreshSort()
		{
			m_dataGrid.Sort(m_dataGrid.SortedColumn, m_dataGrid.SortOrder == SortOrder.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending);
		}

		public Color BackgroundColor
		{
			get { return m_dataGrid.BackgroundColor; }
			set { m_dataGrid.BackgroundColor = value; }
		}

		private void contextMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			if (e.ClickedItem == m_contextMenu_itemDeleteActors)
			{
				RemoveSelectedRows(true);
			}
		}

		private void HandleCellUpdated(DataGridViewCellEventArgs e)
		{
			DataGridViewCellEventHandler handler = CellUpdated;
			if (handler != null)
				handler(m_dataGrid, e);
		}

		private void HandleKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyData == Keys.Delete)
			{
				RemoveSelectedRows(true);
			}
		}

		private void HandleUserAddedRow(object sender, DataGridViewRowEventArgs e)
		{
			DataGridViewRowEventHandler handler = UserAddedRow;
			if (handler != null)
				handler(sender, e);
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
			if (CharacterGroupsWithAssignedActors.Any(cg => cg.VoiceActorId == actor.Id))
			{
				e.CellStyle.Font = m_italicsFont;
				e.CellStyle.ForeColor = Color.Gray;
			}
			else
			{
				e.FormattingApplied = false;
			}
		}

		private void m_actorInformationViewModel_Saved(object sender, EventArgs e)
		{
			if (Saved != null)
				Saved(sender, e);
		}

		private void m_dataGrid_CurrentCellChanged(object sender, EventArgs e)
		{
			if (m_dataGrid.CurrentCell == null)
				return;
			if (m_dataGrid.CurrentCell.OwningColumn.DataPropertyName == "Name")
			{
				m_dataGrid.EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2;
			}
			else
			{
				m_dataGrid.EditMode = DataGridViewEditMode.EditOnEnter;
				//Open combobox (or no effect on regular text box input)
				SendKeys.Send("{F4}");
			}
		}

		private void m_dataGrid_CellEndEdit(object sender, DataGridViewCellEventArgs e)
		{
			HandleCellUpdated(e);
			m_actorInformationViewModel.SaveVoiceActorInformation();
		}

		private void m_dataGrid_SelectionChanged(object sender, EventArgs e)
		{
			EventHandler handler = SelectionChanged;
			if (handler != null)
				handler(sender, e);
		}

		private void m_contextMenu_Opening(object sender, CancelEventArgs e)
		{
			if (SelectedRows.Count > 1)
			{
				m_contextMenu_itemDeleteActors.Text = LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.ContextMenu.DeleteActors", "Delete Actors");
			}
			else
			{
				m_contextMenu_itemDeleteActors.Text = LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.ContextMenu.DeleteActor", "Delete Actor");
			}
		}

		private void m_dataGrid_Leave(object sender, EventArgs e)
		{
			EndEdit();
		}

		private void m_dataGrid_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
		{
			if (e.ColumnIndex == m_dataGrid.Columns["ActorName"].Index)
			{
				if (!m_dataGrid.Rows[e.RowIndex].IsNewRow && string.IsNullOrWhiteSpace(e.FormattedValue.ToString()))
				{
					e.Cancel = true;
					if (!m_inEndEdit)
						MessageBox.Show(LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.InvalidName", "Actor Name must be provided."));
				}
				else if (IsDuplicateActorName(m_dataGrid.Rows[e.RowIndex], e.FormattedValue.ToString()))
				{
					e.Cancel = true;
					if (!m_inEndEdit)
						MessageBox.Show(LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.DuplicateName", "Actor Name must be unique."));
				}
			}
		}

		private bool IsDuplicateActorName(DataGridViewRow modifiedRow, string newActorName)
		{
			foreach (var rowObj in m_dataGrid.Rows)
			{
				DataGridViewRow row = rowObj as DataGridViewRow;
				if (row == null || row.IsNewRow || row == modifiedRow)
					continue;
				if (row.Cells["ActorName"].Value.ToString() == newActorName)
					return true;
			}
			return false;
		}
	}
}
