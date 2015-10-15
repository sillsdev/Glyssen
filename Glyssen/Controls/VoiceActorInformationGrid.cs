using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using L10NSharp;
using SIL.Reporting;

namespace Glyssen.Controls
{
	public partial class VoiceActorInformationGrid : UserControl
	{
		public event EventHandler Saved;
		public event DataGridViewRowEventHandler UserAddedRow;
		public event DataGridViewRowsRemovedEventHandler UserRemovedRows;
		private VoiceActorInformationViewModel m_actorInformationViewModel;
		private bool m_inEndEdit;

		public VoiceActorInformationGrid()
		{
			InitializeComponent();

			m_dataGrid.DataError += m_dataGrid_DataError;

			ActorGender.DataSource = VoiceActorInformationViewModel.GetGenderDataTable();
			ActorGender.ValueMember = "ID";
			ActorGender.DisplayMember = "Name";

			ActorAge.DataSource = VoiceActorInformationViewModel.GetAgeDataTable();
			ActorAge.ValueMember = "ID";
			ActorAge.DisplayMember = "Name";

			ActorQuality.DataSource = VoiceActorInformationViewModel.GetVoiceQualityDataTable();
			ActorQuality.ValueMember = "ID";
			ActorQuality.DisplayMember = "Name";

			// Sadly, we have to do this here because setting it in the Designer doesn't work since BetterGrid overrides
			// the default value in its constructor.
			m_dataGrid.AllowUserToAddRows = true;
			m_dataGrid.MultiSelect = true;
			m_dataGrid.UserAddedRow += HandleUserAddedRow;
		}

		void m_dataGrid_DataError(object sender, DataGridViewDataErrorEventArgs e)
		{
			ErrorReport.ReportFatalException(e.Exception);
		}

		public int RowCount { get { return m_dataGrid.RowCount; } }

		private DataGridViewSelectedRowCollection SelectedRows
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

		public void Initialize(VoiceActorInformationViewModel viewModel, bool sort = true)
		{
			m_actorInformationViewModel = viewModel;
			m_actorInformationViewModel.Saved += m_actorInformationViewModel_Saved;

			m_dataGrid.DataSource = m_actorInformationViewModel.BindingList;

			if (sort)
				m_dataGrid.Sort(m_dataGrid.Columns["ActorName"], ListSortDirection.Ascending);
		}

		private void SaveVoiceActorInformation()
		{
			m_actorInformationViewModel.SaveVoiceActorInformation();
		}

		public DataGridViewEditMode EditMode
		{
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

		private void EndEdit()
		{
			m_inEndEdit = true;
			if (ValidateChildren())
				m_dataGrid.EndEdit();
			m_inEndEdit = false;
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
				MoveToNextField();
		}

		private void RemoveSelectedRows()
		{
			if (m_dataGrid.SelectedRows.Count == 0)
				return;

			var actorsToRemove = new HashSet<VoiceActor.VoiceActor>();
			foreach (DataGridViewRow row in m_dataGrid.SelectedRows)
				actorsToRemove.Add(row.DataBoundItem as VoiceActor.VoiceActor);

			int indexOfFirstRowToRemove = m_dataGrid.SelectedRows[0].Index;
			if (m_actorInformationViewModel.DeleteVoiceActors(actorsToRemove))
			{
				DataGridViewRowsRemovedEventHandler handler = UserRemovedRows;
				if (handler != null)
					handler(m_dataGrid, new DataGridViewRowsRemovedEventArgs(indexOfFirstRowToRemove, m_dataGrid.RowCount));				
			}
		}

		public Color BackgroundColor
		{
			get { return m_dataGrid.BackgroundColor; }
			set { m_dataGrid.BackgroundColor = value; }
		}

		private void contextMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			if (e.ClickedItem == m_contextMenu_itemDeleteActors)
				RemoveSelectedRows();
		}

		private void HandleKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyData == Keys.Delete)
				RemoveSelectedRows();
		}

		private void HandleUserAddedRow(object sender, DataGridViewRowEventArgs e)
		{
			DataGridViewRowEventHandler handler = UserAddedRow;
			if (handler != null)
				handler(sender, e);
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
			SaveVoiceActorInformation();
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

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData == Keys.Enter)
			{
				MoveToNextField();
				return true;
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}

		private void MoveToNextField()
		{
			if (m_dataGrid.Columns.GetLastColumn(DataGridViewElementStates.Visible, DataGridViewElementStates.None) == m_dataGrid.CurrentCell.OwningColumn &&
				m_dataGrid.CurrentRow != null &&
				m_dataGrid.Rows.GetLastRow(DataGridViewElementStates.Visible) == m_dataGrid.CurrentRow.Index)
				return;
			SendKeys.Send("{TAB}");
		}
	}
}
