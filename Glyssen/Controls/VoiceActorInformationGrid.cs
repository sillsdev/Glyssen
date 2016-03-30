using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using DesktopAnalytics;
using Glyssen.Properties;
using Glyssen.Utilities;
using Glyssen.VoiceActor;
using L10NSharp;
using L10NSharp.UI;
using SIL.Reporting;

namespace Glyssen.Controls
{
	public partial class VoiceActorInformationGrid : UserControl
	{
		public event EventHandler Saved;
		public event EventHandler RowCountChanged;
		private VoiceActorInformationViewModel m_actorInformationViewModel;
		private bool m_inEndEdit;
		private int m_selectedActorsRemainingToDelete;

		// See http://stackoverflow.com/questions/937919/datagridviewimagecolumn-red-x
		private class DataGridViewEmptyImageCell : DataGridViewImageCell
		{
			private static readonly Image s_emptyBitmap = new Bitmap(1, 1);
			public static Image EmptyBitmap {get { return s_emptyBitmap; }}

			public override object DefaultNewRowValue { get { return EmptyBitmap; } }
		}

		public VoiceActorInformationGrid()
		{
			InitializeComponent();

			// See http://stackoverflow.com/questions/937919/datagridviewimagecolumn-red-x
			DeleteButtonCol.DefaultCellStyle.NullValue = null;
			DeleteButtonCol.CellTemplate = new DataGridViewEmptyImageCell();

			m_dataGrid.DataError += m_dataGrid_DataError;

			ActorGender.DataSource = VoiceActorInformationViewModel.GetGenderDataTable();
			ActorGender.ValueMember = "ID";
			ActorGender.DisplayMember = "Name";

			ActorAge.DataSource = VoiceActorInformationViewModel.GetAgeDataTable();
			ActorAge.ValueMember = "ID";
			ActorAge.DisplayMember = "Name";
			ActorAge.ToolTipText = LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.ActorAgeTooltip",
				"“Age” quality of actor’s voice");

			ActorQuality.DataSource = VoiceActorInformationViewModel.GetVoiceQualityDataTable();
			ActorQuality.ValueMember = "ID";
			ActorQuality.DisplayMember = "Name";

			ActorInactive.ToolTipText = LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.ActorAgeTooltip",
				"No longer available");

			// Sadly, we have to do this here because setting it in the Designer doesn't work since BetterGrid overrides
			// the default value in its constructor.
			m_dataGrid.AllowUserToAddRows = true;
			m_dataGrid.AllowUserToDeleteRows = true;
			m_dataGrid.MultiSelect = true;
			m_dataGrid.EditMode = DataGridViewEditMode.EditOnEnter;

			// We can't set this in the designer because L10NSharp is squashing it when the header is localized.
			Cameo.ToolTipText = LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.CameoTooltip",
				"Distinguished actor to play minor character role.");

			LocalizeItemDlg.StringsLocalized += HandleStringsLocalized;
		}

		private void HandleStringsLocalized()
		{
			ActorGender.DataSource = VoiceActorInformationViewModel.GetGenderDataTable();
			ActorAge.DataSource = VoiceActorInformationViewModel.GetAgeDataTable();
			ActorQuality.DataSource = VoiceActorInformationViewModel.GetVoiceQualityDataTable();
		}

		void m_dataGrid_DataError(object sender, DataGridViewDataErrorEventArgs e)
		{
			Analytics.ReportException(e.Exception);
			ErrorReport.ReportFatalException(e.Exception);
			throw e.Exception;
		}

		public int RowCount
		{
			get { return m_dataGrid.RowCount; }
			private set
			{
				if (m_dataGrid.RowCount == value || value == 0)
					return;
				m_dataGrid.RowCount = value;
				OnRowCountChanged();
			}
		}

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

		public void Initialize(VoiceActorInformationViewModel viewModel, bool sortByName)
		{
			m_actorInformationViewModel = viewModel;
			m_actorInformationViewModel.Sort(sortByName ? VoiceActorsSortedBy.Name : VoiceActorsSortedBy.OrderEntered, true);
			RowCount = m_actorInformationViewModel.Actors.Count + 1;

			m_actorInformationViewModel.Saved += m_actorInformationViewModel_Saved;
		}

		private void DeleteActor(int iRow)
		{
			foreach (DataGridViewRow row in m_dataGrid.Rows)
				row.Selected = row.Index == iRow;
			RemoveSelectedRows();
		}

		private void SaveVoiceActorInformation()
		{
			m_actorInformationViewModel.SaveVoiceActorInformation();
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			if (ParentForm != null)
				ParentForm.Closing += ParentForm_Closing;
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			if (Visible)
				SetBackgroundColorToAvoidScrollbarHangingBelowGrid();
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

			m_selectedActorsRemainingToDelete = GetCountOfConfirmedActorsToDelete();

			if (m_selectedActorsRemainingToDelete == 0)
				return;

			foreach (DataGridViewRow row in m_dataGrid.SelectedRows)
				if (!row.IsNewRow)
					m_dataGrid.Rows.Remove(row);
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
			// PG-639: has the new row been removed by m_dataGrid.CancelEdit?
			if (e.RowIndex == m_dataGrid.RowCountLessNewRow)
				return;

			if (m_actorInformationViewModel.ValidateActor(e.RowIndex) == VoiceActorInformationViewModel.ActorValidationState.Valid)
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

		private void m_dataGrid_Enter(object sender, EventArgs e)
		{
			if (m_dataGrid.CurrentCellAddress.Y == m_actorInformationViewModel.Actors.Count)
				m_dataGrid_NewRowNeeded(sender, new DataGridViewRowEventArgs(m_dataGrid.Rows[m_dataGrid.CurrentCellAddress.Y]));
		}

		private void m_dataGrid_Leave(object sender, EventArgs e)
		{
			EndEdit();
		}

		private void m_dataGrid_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
		{
			if (e.ColumnIndex == ActorName.Index)
			{
				if (m_dataGrid.Rows[e.RowIndex].IsNewRow)
					return;

				VoiceActor.VoiceActor editedActor = e.RowIndex < m_actorInformationViewModel.Actors.Count
					? m_actorInformationViewModel.Actors[e.RowIndex]
					: null;
				Debug.Assert(editedActor != null);

				if (!string.IsNullOrWhiteSpace(e.FormattedValue.ToString()) &&
					m_actorInformationViewModel.IsDuplicateActorName(editedActor, e.FormattedValue.ToString()))
				{
					e.Cancel = true;
					if (!m_inEndEdit)
						MessageBox.Show(LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.DuplicateName", "Actor Name must be unique."));
				}
			}
		}

		private void m_dataGrid_RowValidating(object sender, DataGridViewCellCancelEventArgs e)
		{
			if (m_dataGrid.Rows[e.RowIndex].IsNewRow)
			{
				if (m_actorInformationViewModel.Actors.Count == e.RowIndex + 1)
				{
					var actorsToRemove = new HashSet<VoiceActor.VoiceActor>();
					actorsToRemove.Add(m_actorInformationViewModel.Actors[e.RowIndex]);
					m_actorInformationViewModel.DeleteVoiceActors(actorsToRemove);
					m_dataGrid.CancelEdit();
				}
				return;
			}

			if (m_actorInformationViewModel.ValidateActor(e.RowIndex) == VoiceActorInformationViewModel.ActorValidationState.NoName)
			{
				e.Cancel = true;
				if (!m_inEndEdit)
				{
					MessageBox.Show(LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.InvalidName",
						"Actor Name must be provided."));
					if (m_dataGrid.CurrentCellAddress.X != ActorName.Index)
						m_dataGrid.CurrentCell = m_dataGrid.Rows[e.RowIndex].Cells[ActorName.Index];
				}
			}
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

		private void m_dataGrid_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
		{
			if (e.RowIndex >= m_actorInformationViewModel.Actors.Count)
				return;

			var actor = m_actorInformationViewModel.Actors[e.RowIndex];
			if (e.ColumnIndex == ActorName.Index)
				e.Value = actor.Name;
			else if (e.ColumnIndex == ActorGender.Index)
				e.Value = actor.Gender;
			else if (e.ColumnIndex == ActorAge.Index)
				e.Value = actor.Age;
			else if (e.ColumnIndex == ActorStatus.Index)
				e.Value = actor.Status;
			else if (e.ColumnIndex == ActorQuality.Index)
				e.Value = actor.VoiceQuality;
			else if (e.ColumnIndex == Cameo.Index)
				e.Value = actor.IsCameo;
			else if (e.ColumnIndex == ActorInactive.Index)
				e.Value = actor.IsInactive;
			else if (e.ColumnIndex == DeleteButtonCol.Index)
			{
				if (m_dataGrid.Rows[e.RowIndex].IsNewRow)
					e.Value = DataGridViewEmptyImageCell.EmptyBitmap;
				else
				{
					var rc = m_dataGrid.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
					bool mouseOverRemoveImage = rc.Contains(PointToClient(MousePosition));

					if (m_dataGrid.CurrentCellAddress.Y == e.RowIndex || mouseOverRemoveImage)
						e.Value = (mouseOverRemoveImage ? Resources.RemoveGridRowHot : DeleteButtonCol.Image);
				}
			}
		}

		private void m_dataGrid_CellValuePushed(object sender, DataGridViewCellValueEventArgs e)
		{
			VoiceActor.VoiceActor actor = m_actorInformationViewModel.Actors[e.RowIndex];

			if (e.Value == null)
				return;

			if (e.ColumnIndex == ActorName.Index)
				actor.Name = (string)e.Value;
			else if (e.ColumnIndex == ActorGender.Index)
				actor.Gender = (ActorGender)e.Value;
			else if (e.ColumnIndex == ActorAge.Index)
				actor.Age = (ActorAge)e.Value;
			else if (e.ColumnIndex == ActorStatus.Index)
				actor.Status = (bool)e.Value;
			else if (e.ColumnIndex == ActorQuality.Index)
				actor.VoiceQuality = (VoiceQuality)e.Value;
			else if (e.ColumnIndex == Cameo.Index)
				actor.IsCameo = (bool)e.Value;
			else if (e.ColumnIndex == ActorInactive.Index)
				m_actorInformationViewModel.SetInactive(actor, (bool)e.Value);
		}

		private void m_dataGrid_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
		{
			if (m_selectedActorsRemainingToDelete > 0)
			{
				m_selectedActorsRemainingToDelete--;
				var actorsToRemove = new HashSet<VoiceActor.VoiceActor>();
				actorsToRemove.Add(m_actorInformationViewModel.Actors[e.RowIndex]);
				m_actorInformationViewModel.DeleteVoiceActors(actorsToRemove);
			}
			OnRowCountChanged(e);
		}

		private void m_dataGrid_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
		{
			OnRowCountChanged(e);
		}

		private void OnRowCountChanged(EventArgs e = null)
		{
			SetBackgroundColorToAvoidScrollbarHangingBelowGrid();

			if (RowCountChanged != null)
				RowCountChanged(m_dataGrid, e ?? new EventArgs());
		}

		private void HandleUserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
		{
			if (e.Row.Index >= m_actorInformationViewModel.Actors.Count)
				return; // Deleting new row by pressing escape - can't cancel this.

			if (m_selectedActorsRemainingToDelete == 0)
			{
				m_selectedActorsRemainingToDelete = GetCountOfConfirmedActorsToDelete();
				if (m_selectedActorsRemainingToDelete == 0)
					e.Cancel = true;
			}
		}

		private int GetCountOfConfirmedActorsToDelete()
		{
			var actorsToRemove = new HashSet<VoiceActor.VoiceActor>();
			foreach (DataGridViewRow row in m_dataGrid.SelectedRows)
				actorsToRemove.Add(m_actorInformationViewModel.Actors[row.Index]);

			int countOfActorsToDelete = actorsToRemove.Count;
			int countOfAssignedActorsToDelete = m_actorInformationViewModel.CountOfAssignedActors(actorsToRemove);

			string msg;
			string title;

			if (countOfAssignedActorsToDelete > 0)
			{
				if (countOfActorsToDelete > 1)
				{
					msg =
						LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.DeleteAssignedActorsDialog.MessagePlural",
							"One or more of the selected actors is assigned to a character group. Deleting the actor will remove the assignment as well. Are you sure you want to delete the selected actors?");
					title =
						LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.DeleteAssignedActorsDialog.TitlePlural",
							"Voice Actors Assigned");
				}
				else
				{
					msg =
						LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.DeleteAssignedActorsDialog.MessageSingular",
							"The selected actor is assigned to a character group. Deleting the actor will remove the assignment as well. Are you sure you want to delete the selected actor?");
					title =
						LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.DeleteAssignedActorsDialog.TitleSingular",
							"Voice Actor Assigned");
				}
			}
			else
			{
				title = LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.DeleteRowsDialog.Title", "Confirm");

				if (countOfActorsToDelete > 1)
				{
					msg = LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.DeleteRowsDialog.MessagePlural",
						"Are you sure you want to delete the selected actors?");
				}
				else
				{
					msg = LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.DeleteRowsDialog.MessageSingular",
						"Are you sure you want to delete the selected actor?");
				}
			}
			return MessageBox.Show(msg, title, MessageBoxButtons.YesNo) == DialogResult.No ? 0 : actorsToRemove.Count;
		}

		private void m_dataGrid_NewRowNeeded(object sender, DataGridViewRowEventArgs e)
		{
			if (m_actorInformationViewModel.Actors.Count == e.Row.Index)
				m_actorInformationViewModel.AddNewActor();
		}

		private void m_dataGrid_CellContentClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.ColumnIndex == DeleteButtonCol.Index && e.RowIndex >= 0 && !m_dataGrid.Rows[e.RowIndex].IsNewRow)
				DeleteActor(e.RowIndex);
		}

		private void m_dataGrid_CellMouseEnterOrLeave(object sender, DataGridViewCellEventArgs e)
		{
			if (e.ColumnIndex == DeleteButtonCol.Index && e.RowIndex >= 0 && e.RowIndex < m_actorInformationViewModel.Actors.Count)
				m_dataGrid.InvalidateCell(e.ColumnIndex, e.RowIndex);
		}

		private void m_dataGrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
		{
			if (m_actorInformationViewModel == null || e.RowIndex < 0 || e.RowIndex >= m_actorInformationViewModel.Actors.Count)
				return;

			VoiceActor.VoiceActor actor = m_actorInformationViewModel.Actors[e.RowIndex];
			if (actor.IsInactive)
				e.CellStyle.ForeColor = Color.Gray;
		}

		private void HandleResize(object sender, EventArgs e)
		{
			SetBackgroundColorToAvoidScrollbarHangingBelowGrid();
		}

		private void SetBackgroundColorToAvoidScrollbarHangingBelowGrid()
		{
			m_dataGrid.BackgroundColor = (m_dataGrid.VScrollBar.Visible) ? SystemColors.Window : GlyssenColorPalette.ColorScheme.BackColor;
		}
	}
}
