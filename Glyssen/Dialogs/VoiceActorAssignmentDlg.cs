﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text;
using System.Windows.Forms;
using DesktopAnalytics;
using Glyssen.Controls;
using Glyssen.Properties;
using Glyssen.Utilities;
using GlyssenCharacters;
using GlyssenEngine;
using GlyssenEngine.Casting;
using GlyssenEngine.Character;
using GlyssenEngine.Export;
using GlyssenEngine.ViewModels;
using L10NSharp;
using SIL.Reporting;
using SIL.Extensions;
using static System.String;
using Resources = Glyssen.Properties.Resources;

namespace Glyssen.Dialogs
{
	public partial class VoiceActorAssignmentDlg : FormWithPersistedSettings, ILocalizable
	{
		private const string kCreateNewGroupMenuItemId = "CreateNewGroup";
		private const string kAssignToCameoActorItemId = "AssignToCameoActor";
		private const string kMoveToAnotherGroupMenuItemId = "MoveToAnotherGroup";
		private readonly VoiceActorAssignmentViewModel m_actorAssignmentViewModel;
		private readonly Project m_project;
		private DataGridViewColumn m_sortedColumn;
		private bool m_sortedAscending;
		private bool m_selectingInResponseToDataChange;
		private List<string> m_characterIdsForSelectedGroup;
		private bool m_characterDetailsVisible;
		//private string m_fmtNoCharactersInGroup;
		private readonly Font m_hyperlinkFont;
		private string m_fmtMoveCharactersInfo;
		private string m_fmtHideCharacterDetails;
		private string m_fmtShowCharacterDetails;
		private string m_fmtMatches;
		private List<string> m_pendingMoveCharacters;
		private readonly BackgroundWorker m_findCharacterBackgroundWorker;
		private readonly BackgroundWorker m_determineMatchingCharactersBackgroundWorker;
		private volatile SortedSet<Tuple<int, int>> m_currentMatches;
		private bool m_programmaticClickOfUpdateGroups;
		private bool m_detailPinned;

		public VoiceActorAssignmentDlg(VoiceActorAssignmentViewModel viewModel)
		{
			InitializeComponent();

			m_characterGroupGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
			m_characterDetailsGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
			m_characterDetailsGrid.RowCount = 0;

			m_project = viewModel.Project;

			m_menuItemCreateNewGroup.Tag = kCreateNewGroupMenuItemId;
			m_menuItemAssignToCameoActor.Tag = kAssignToCameoActorItemId;
			m_menuItemMoveToAnotherGroup.Tag = kMoveToAnotherGroupMenuItemId;

			m_actorAssignmentViewModel = viewModel;
			m_actorAssignmentViewModel.Saved += HandleModelSaved;

			VoiceActorCol.DataSource = GetMultiColumnActorDataTable(null);
			VoiceActorCol.ValueMember = "ID";
			VoiceActorCol.DisplayMember = "Name";
			VoiceActorCol.GetSpecialDropDownImageToDraw += VoiceActorCol_GetSpecialDropDownImageToDraw;
			VoiceActorCol.Name = "VoiceActorCol";

			// Sadly, we have to do this here because setting it in the Designer doesn't work since BetterGrid overrides
			// the default value in its constructor.
			m_characterGroupGrid.MultiSelect = true;
			m_characterDetailsGrid.MultiSelect = true;

			HandleStringsLocalized();
			Program.RegisterLocalizable(this);

			m_findCharacterBackgroundWorker = new BackgroundWorker { WorkerSupportsCancellation = true };
			m_findCharacterBackgroundWorker.DoWork += FindCharacter;
			m_findCharacterBackgroundWorker.RunWorkerCompleted += FindCharacterCompleted;

			m_determineMatchingCharactersBackgroundWorker = new BackgroundWorker { WorkerSupportsCancellation = true };
			m_determineMatchingCharactersBackgroundWorker.DoWork += GetInidicesOfMatchingCharacters;
			m_determineMatchingCharactersBackgroundWorker.RunWorkerCompleted += GetCountOfMatchingCharactersCompleted;

			m_hyperlinkFont = new Font(m_characterGroupGrid.Columns[CharacterIdsCol.Index].InheritedStyle.Font, FontStyle.Underline);

			adjustGroupsToMatchMyVoiceActorsToolStripMenuItem.Enabled = m_project.VoiceActorList.ActiveActors.Any();
		}

		public bool LaunchCastSizePlanningUponExit { get; private set; }

		private void VoiceActorAssignmentDlg_Load(object sender, EventArgs e)
		{
			TileFormLocation();
		}

		/// <summary>
		/// Sets the row count if necessary; otherwise (optionally) refreshes the grid.
		/// </summary>
		/// <param name="refreshOnlyIfNeeded">Indicates whether a full refresh is desired in cases where the row count has not changed</param>
		/// <returns>A value indicating whether the grid was refreshed (always true if the row count changed)</returns>
		private bool SetRowCount(bool refreshOnlyIfNeeded = false)
		{
			bool changingRowCount = m_actorAssignmentViewModel.CharacterGroups.Count != m_characterGroupGrid.RowCount;
			if (changingRowCount)
			{
				var sizeRestoreInfo = SaveAutoSizeInfo();
				if (m_actorAssignmentViewModel.CharacterGroups.Any())
					m_characterGroupGrid.ClearSelection();
				m_characterGroupGrid.RowCount = m_actorAssignmentViewModel.CharacterGroups.Count;
				// Need to clear the selection here again because some of the property setters on
				// DataGridView have the side-effect of creating a selection. We want to avoid having
				// HandleDataGridViewBlocksCellValueNeeded get called with an index that is out of
				// range for the new book.
				m_characterGroupGrid.ClearSelection();
				RestoreAutoSizeInfo(sizeRestoreInfo);
			}
			else if (!refreshOnlyIfNeeded)
				m_characterGroupGrid.Refresh();
			else
				return false;

			if (m_characterDetailsVisible)
				SetCharacterDetailsPanePercentage();

			return true;
		}

		private Tuple<Dictionary<DataGridViewColumn, DataGridViewAutoSizeColumnMode>, DataGridViewAutoSizeRowsMode> SaveAutoSizeInfo()
		{
			var columnSizeModesToRestore = m_characterGroupGrid.Columns.OfType<DataGridViewColumn>().Where(c => c.AutoSizeMode != DataGridViewAutoSizeColumnMode.None).ToDictionary(c => c, c => c.AutoSizeMode);
			var autoSizeRowsModeToRestore = m_characterGroupGrid.AutoSizeRowsMode;
			foreach (var column in columnSizeModesToRestore.Keys)
				column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
			m_characterGroupGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
			return new Tuple<Dictionary<DataGridViewColumn, DataGridViewAutoSizeColumnMode>, DataGridViewAutoSizeRowsMode>(
				columnSizeModesToRestore, autoSizeRowsModeToRestore);
		}

		private void RestoreAutoSizeInfo(Tuple<Dictionary<DataGridViewColumn, DataGridViewAutoSizeColumnMode>, DataGridViewAutoSizeRowsMode> info)
		{
				var columnSizeModesToRestore = info.Item1;
				var autoSizeRowsModeToRestore = info.Item2;

				foreach (var kvp in columnSizeModesToRestore)
					kvp.Key.AutoSizeMode = kvp.Value;
				m_characterGroupGrid.AutoSizeRowsMode = autoSizeRowsModeToRestore;
			}

		public void HandleStringsLocalized()
		{
			m_undoButton.Tag = new Tuple<string, Keys>(m_undoButton.ToolTipText, Keys.Z);
			m_redoButton.Tag = new Tuple<string, Keys>(m_redoButton.ToolTipText, Keys.Y);

			if (!m_undoButton.Enabled)
				SetUndoOrRedoButtonToolTip(m_undoButton, null);
			if (!m_redoButton.Enabled)
				SetUndoOrRedoButtonToolTip(m_redoButton, null);

			//m_fmtNoCharactersInGroup = m_lblHowToAssignCharactersToCameoGroup.Text;
			m_fmtMoveCharactersInfo = m_lblMovePendingInfo.Text;
			m_fmtHideCharacterDetails = LocalizationManager.GetString(
					"DialogBoxes.VoiceActorAssignmentDlg.HideCharacterDetailsLink", "Hide details for {0} group");
			m_fmtShowCharacterDetails = m_linkLabelShowHideDetails.Text;
			m_fmtMatches = m_lblMatches.Text;
			UpdateMatchLabelDisplay();

			Text = Format(Text, m_project.Name);

			string printNonLinkText = m_linkPrint.Text;
			string printLinkText = LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.Instructions.Line1.LinkText", "Print");
			m_linkPrint.Links.Clear();
			m_linkPrint.Links.Add(printNonLinkText.IndexOf("{0}", StringComparison.Ordinal), printLinkText.Length);
			m_linkPrint.Text = Format(printNonLinkText, printLinkText);
		}

		private Image VoiceActorCol_GetSpecialDropDownImageToDraw(DataGridViewMultiColumnComboBoxColumn sender, int rowIndex)
		{
			return m_characterGroupGrid.Rows[rowIndex].Cells[sender.Index].ReadOnly ? Resources.CameoStar : null;
		}

		public CharacterGroup FirstSelectedCharacterGroup
		{
			get
			{
				if (m_characterGroupGrid.SelectedRows.Count == 0)
					return null;
				return m_actorAssignmentViewModel.CharacterGroups[m_characterGroupGrid.SelectedRows[0].Index];
			}
		}

		private void UnAssignActorsFromSelectedGroups()
		{
			m_actorAssignmentViewModel.UnAssignActorFromGroups(SelectedGroupsThatCanBeUnassigned);
		}

		private void HandleEditVoiceActorsClick(object sender, EventArgs e)
		{
			var actorInfoViewModel = new VoiceActorInformationViewModel(m_project);

			var sizeRestoreInfo = SaveAutoSizeInfo();

			using (var actorDlg = new VoiceActorInformationDlg(actorInfoViewModel, false, true))
			{
				MainForm.LogDialogDisplay(actorDlg);
				var result = actorDlg.ShowDialog(this);

				m_actorAssignmentViewModel.NoteActorChanges(actorInfoViewModel.Changes);
				if (actorInfoViewModel.DataHasChangedInWaysThatMightAffectGroupGeneration)
				{
					if (result == DialogResult.OK)
					{
						m_programmaticClickOfUpdateGroups = true;
						HandleUpdateGroupsClick(actorDlg, e);
					}

					VoiceActorCol.DataSource = GetMultiColumnActorDataTable(null);

					SetVoiceActorCellDataSource();
				}
				adjustGroupsToMatchMyVoiceActorsToolStripMenuItem.Enabled = actorInfoViewModel.ActiveActors.Any();
			}

			RestoreAutoSizeInfo(sizeRestoreInfo);
		}

		private void HandlePrintClick(object sender, EventArgs e)
		{
			MessageBox.Show("This feature has not been implemented yet. Choose File -> Save As instead.");
		}

		private void m_unAssignActorFromGroupToolStripMenuItem_Click(object sender, EventArgs e)
		{
			UnAssignActorsFromSelectedGroups();
		}

		private void HandleSplitSelectedGroupClick(object sender, EventArgs e)
		{
			using (var splitGroupDlg = new SplitCharacterGroupDlg(FirstSelectedCharacterGroup, m_actorAssignmentViewModel))
			{
				MainForm.LogDialogDisplay(splitGroupDlg);
				if (splitGroupDlg.ShowDialog(this) == DialogResult.OK)
				{
					var newGroupIndex = m_actorAssignmentViewModel.CharacterGroups.IndexOf(splitGroupDlg.NewGroup);
					m_characterGroupGrid.CurrentCell = m_characterGroupGrid.Rows[newGroupIndex].Cells[CharacterIdsCol.Name];
				}
			}
		}

		private void HandleAddCharacterToGroupClick(object sender, EventArgs e)
		{
			AddCharacterToGroup(FirstSelectedCharacterGroup);
		}

		private void m_contextMenuCharacterGroups_Opening(object sender, CancelEventArgs e)
		{
			bool multipleGroupsSelected = m_characterGroupGrid.SelectedRows.Count > 1;

			m_unAssignActorFromGroupToolStripMenuItem.Text = multipleGroupsSelected
				? LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.RemoveVoiceActorAssignments",
					"Remove Voice Actor Assignments")
				: LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.RemoveVoiceActorAssignment",
					"Remove Voice Actor Assignment");

			m_unAssignActorFromGroupToolStripMenuItem.Enabled = SelectedGroupsThatCanBeUnassigned.Any();
			m_splitGroupToolStripMenuItem.Enabled = m_characterGroupGrid.SelectedRows.Count == 1 && FirstSelectedCharacterGroup.CharacterIds.Count > 1;
			m_AddCharacterToGroupToolStripMenuItem.Enabled = m_characterGroupGrid.SelectedRows.Count == 1;
		}

		private IEnumerable<CharacterGroup> SelectedGroupsThatCanBeUnassigned
		{
			get
			{
				for (int i = 0; i < m_characterGroupGrid.SelectedRows.Count; i++)
				{
					var group = m_actorAssignmentViewModel.CharacterGroups[m_characterGroupGrid.SelectedRows[i].Index];
					if (m_actorAssignmentViewModel.CanRemoveAssignment(group))
						yield return group;
				}
			}
		}

		private void m_contextMenuCharacters_Opening(object sender, CancelEventArgs e)
		{
			// Can't use m_contextMenuCharacters or m_tmiCreateNewGroup here because for some reason the ones displaying are copies of those
			ContextMenuStrip cms = sender as ContextMenuStrip;
			if (cms == null)
				return;
			var selectedCharacterGroup = FirstSelectedCharacterGroup;
			foreach (var item in cms.Items.OfType<ToolStripMenuItem>())
			{
				switch (item.Tag.ToString())
				{
					case kCreateNewGroupMenuItemId:
						// Don't let the user do this unless the group left behind would still be viable (either assigned to a cameo actor or having
						// some characters still in it.
						item.Enabled = selectedCharacterGroup.AssignedToCameoActor ||
										m_characterDetailsGrid.SelectedRows.Count < m_characterDetailsGrid.RowCount;
						item.Text = m_characterDetailsGrid.SelectedRows.Count > 1
							? LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.ContextMenus.CreateNewGroupWithCharacters",
								"Create a new group with the selected characters")
							: m_menuItemCreateNewGroup.Text;
						break;
					case kAssignToCameoActorItemId:
						var actorOfSelectedGroup = selectedCharacterGroup.VoiceActorId;
						var availableCameoActors = m_project.VoiceActorList.ActiveActors.Where(a => a.Id != actorOfSelectedGroup && a.IsCameo)
							.OrderBy(a => a.Name).ToList();

						item.Enabled = availableCameoActors.Any();
						item.Text = m_characterDetailsGrid.SelectedRows.Count > 1
							? LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.AssignSelectedCharactersToCameoActor",
								"Assign selected characters to cameo actor")
							: m_menuItemAssignToCameoActor.Text;

						item.DropDownItems.Clear();

						if (item.Enabled)
						{
							foreach (var cameoActor in availableCameoActors)
							{
								var subMenu = new ToolStripMenuItem(cameoActor.Name, null, HandleAssignToCameoActorClick);
								subMenu.Tag = cameoActor.Id;
								item.DropDownItems.Add(subMenu);
							}
						}
						break;
					case kMoveToAnotherGroupMenuItemId:
						item.Text = m_characterDetailsGrid.SelectedRows.Count > 1
							? LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.MoveSelectedCharactersToAnotherGroup",
								"Move selected characters to another group...")
							: m_menuItemMoveToAnotherGroup.Text;
						break;
				}
			}
		}

		private void m_menuItemCreateNewGroup_Click(object sender, EventArgs e)
		{
			IEnumerable<string> localizedCharacterIds = m_characterDetailsGrid.SelectedRows.Cast<DataGridViewRow>().Select(r => (string)r.Cells[CharacterDetailsIdCol.Index].Value);
			var characterIds = localizedCharacterIds.Select(lc => CharacterVerseData.SingletonLocalizedCharacterIdToCharacterIdDictionary[lc]);

			if (m_actorAssignmentViewModel.MoveCharactersToGroup(characterIds.ToList(), null))
			{
				m_characterGroupGrid.CurrentCell = m_characterGroupGrid.Rows[m_characterGroupGrid.RowCount-1].Cells[CharacterIdsCol.Name];
			}
		}

		private void HandleAssignToCameoActorClick(object sender, EventArgs e)
		{
			var menuItem = (ToolStripMenuItem)sender;
			IEnumerable<string> localizedCharacterIds = m_characterDetailsGrid.SelectedRows.Cast<DataGridViewRow>().Select(r => (string)r.Cells[CharacterDetailsIdCol.Index].Value);
			var characterIds = localizedCharacterIds.Select(lc => CharacterVerseData.SingletonLocalizedCharacterIdToCharacterIdDictionary[lc]);

			var cameoGroupIndex = m_actorAssignmentViewModel.CharacterGroups.IndexOf(g => g.VoiceActorId == (int) menuItem.Tag);

			if (m_actorAssignmentViewModel.MoveCharactersToGroup(characterIds.ToList(),
				m_actorAssignmentViewModel.CharacterGroups[cameoGroupIndex], ConfirmCriticalProximityDegradation))
			{
				// Need to get this again because a group higher up in the list might have been deleted as a side-effect of the move.
				cameoGroupIndex = m_actorAssignmentViewModel.CharacterGroups.IndexOf(g => g.VoiceActorId == (int)menuItem.Tag);
				m_characterGroupGrid.CurrentCell = m_characterGroupGrid.Rows[cameoGroupIndex].Cells[CharacterIdsCol.Name];
			}
		}

		private bool ConfirmCriticalProximityDegradation(int newProximity, string firstCharacterId, string secondCharacterId,
			string firstReference, string secondReference, string destGroupId, IReadOnlyList<string> characterIds)
		{
			var dlgMessageFormat1 = (firstReference == secondReference) ?
				LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.MoveCharacterDialog.Message.Part1SameRef",
					"This move will result in the same voice actor speaking the parts of both [{1}] and [{2}] in {3}. This is not ideal. (Proximity: {0})") :
				LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.MoveCharacterDialog.Message.Part1DifferentRef",
					"This move will result in the same voice actor speaking the parts of both [{1}] in {3} and [{2}] in {4}. This is not ideal. (Proximity: {0})");
			var dlgMessagePart1 = string.Format(dlgMessageFormat1,
				newProximity,
				firstCharacterId,
				secondCharacterId,
				firstReference, secondReference);

			Logger.WriteEvent($"{dlgMessagePart1}\r\n>>>Moving {characterIds.Count} characters to group {destGroupId}:\r\n   {String.Join("\r\n   ", characterIds)}");

			var dlgMessagePart2 =
				LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.MoveCharacterDialog.Message.Part2",
					"Do you want to continue with this move?");

			var dlgMessage = dlgMessagePart1 + Environment.NewLine + Environment.NewLine + dlgMessagePart2;
			var dlgTitle = LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.MoveCharacterDialog.Title",
				"Confirm");

			if (MessageBox.Show(dlgMessage, dlgTitle, MessageBoxButtons.YesNo) != DialogResult.Yes)
			{
				Logger.WriteEvent("User cancelled move of characters.");
				return false;
			}

			return true;
		}

		private void m_menuItemMoveToAnotherGroup_Click(object sender, EventArgs e)
		{
			var localizedCharacterIds = m_characterDetailsGrid.SelectedRows.Cast<DataGridViewRow>().Select(r => (string)r.Cells[CharacterDetailsIdCol.Index].Value).ToList();
			m_pendingMoveCharacters = localizedCharacterIds.Select(lc => CharacterVerseData.SingletonLocalizedCharacterIdToCharacterIdDictionary[lc]).ToList();
			UpdateUiForPendingCharacterMove(localizedCharacterIds);
		}

		private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using (var dlg = new RolesForVoiceActorsSaveAsDialog(new ProjectExporter(m_project, GlyssenSettingsProvider.ExportSettingsProvider)))
				dlg.ShowDialog(this);
		}

		private void UpdateUiForPendingCharacterMove(List<string> localizedCharacterIds = null)
		{
			bool movePending = localizedCharacterIds != null;
			if (movePending)
			{
				StringBuilder charactersBldr = new StringBuilder(Environment.NewLine);
				charactersBldr.Append(m_pendingMoveCharacters.First());
				if (m_pendingMoveCharacters.Count > 1)
				{
					charactersBldr.Append(Environment.NewLine);
					charactersBldr.AppendFormat("and {0} additional characters", m_pendingMoveCharacters.Count - 1);
				}
				m_lblMovePendingInfo.Text = Format(m_fmtMoveCharactersInfo, charactersBldr);
			}
			m_tableLayoutPanelMove.Visible = movePending;
			m_tableLayoutPanelVoiceActorList.Visible = !movePending;
			// TODO: Try to make this work ---> VoiceActorCol.ReadOnly = movePending;
			m_characterGroupGrid.Columns[VoiceActorCol.Index].ReadOnly = true;
			m_linkLabelShowHideDetails.Enabled = !movePending;
			m_btnMove.Enabled = false;
			if (movePending)
			{
				foreach (DataGridViewRow row in m_characterGroupGrid.Rows)
					row.Selected = false;
				m_characterGroupGrid.CurrentCell = null;
			}
			else if (m_characterGroupGrid.CurrentCell == null)
			{
				for (int i = 0; i < m_actorAssignmentViewModel.CharacterGroups.Count; i++)
				{
					if (m_actorAssignmentViewModel.CharacterGroups[i].CharacterIds.Contains(m_pendingMoveCharacters.First()))
					{
						m_characterGroupGrid.CurrentCell = m_characterGroupGrid.Rows[i].Cells[CharacterIdsCol.Index];
						return;
					}
				}
			}
		}

		private void m_btnMove_Click(object sender, EventArgs e)
		{
			int rowIndexOfTargetGroup = m_characterGroupGrid.SelectedRows[0].Index;
			var selectedGroup = m_actorAssignmentViewModel.CharacterGroups[rowIndexOfTargetGroup];

			if (m_actorAssignmentViewModel.MoveCharactersToGroup(m_pendingMoveCharacters, selectedGroup, ConfirmCriticalProximityDegradation))
			{
				// Need to get this again because a group higher up in the list might have been deleted as a side-effect of the move.
				rowIndexOfTargetGroup = m_actorAssignmentViewModel.CharacterGroups.IndexOf(selectedGroup);
				m_characterGroupGrid.CurrentCell = m_characterGroupGrid.Rows[rowIndexOfTargetGroup].Cells[CharacterIdsCol.Name];
			}
			UpdateUiForPendingCharacterMove();
			m_pendingMoveCharacters = null;
			m_characterGroupGrid.Invalidate();
		}

		private void m_btnCancelMove_Click(object sender, EventArgs e)
		{
			UpdateUiForPendingCharacterMove();
		}

		private void m_characterGroupGrid_CellContentClick(object sender, DataGridViewCellEventArgs e)
		{
			// ignore clicks on the header row
			if (e.RowIndex < 0) return;

			if (e.RowIndex >= m_actorAssignmentViewModel.CharacterGroups.Count || e.ColumnIndex != CharacterIdsCol.Index)
				return;
			var group = m_actorAssignmentViewModel.CharacterGroups[e.RowIndex];
			if (group != null && group.AssignedToCameoActor)
			{
				if (!group.CharacterIds.Any())
				{
					AddCharacterToGroup(group);
					m_characterGroupGrid[e.ColumnIndex, e.RowIndex] = new DataGridViewTextBoxCell();
				}
			}
		}

		private void AddCharacterToGroup(CharacterGroup characterGroup)
		{
			var model = new AddCharactersToGroupViewModel(m_project.AllCharacterDetailDictionary, m_project.KeyStrokesByCharacterId, characterGroup.CharacterIds,
				characterGroup.AssignedToCameoActor ? m_project.VoiceActorList.GetVoiceActorById(characterGroup.VoiceActorId) : null);
			using (var dlg = new AddCharacterToGroupDlg(model))
			{
				MainForm.LogDialogDisplay(dlg);
				if (dlg.ShowDialog(this) == DialogResult.OK)
				{
					if (m_actorAssignmentViewModel.MoveCharactersToGroup(dlg.SelectedCharacters.ToReadOnlyList(), characterGroup, ConfirmCriticalProximityDegradation))
					{
						var rowIndexOfTargetGroup = m_actorAssignmentViewModel.CharacterGroups.IndexOf(characterGroup);
						m_characterGroupGrid.CurrentCell = m_characterGroupGrid.Rows[rowIndexOfTargetGroup].Cells[CharacterIdsCol.Name];
					}
				}
			}
		}

		private void HandleCastSizePlanClick(object sender, EventArgs e)
		{
			LaunchCastSizePlanningUponExit = true;
			DialogResult = DialogResult.OK;
			Close();
		}

		private void HandleUpdateGroupsClick(object sender, EventArgs e)
		{
			// The UI should not allow this (by disabling buttons/menu options)
			if (!m_project.VoiceActorList.ActiveActors.Any())
				return;

			// REVIEW: When they regenerate, which group should be selected? The one with the same ID or the one containing the same major character.
			// Used to do the latter, but now we do the former.
			var idOfSelectedGroup = (m_characterGroupGrid.SelectedRows.Count == 1)
				? FirstSelectedCharacterGroup.GroupId : null;

			m_actorAssignmentViewModel.RegenerateGroups(() => {
				GenerateGroupsProgressDialog.GenerateGroupsWithProgress(m_project,
					true, false, true, null, m_programmaticClickOfUpdateGroups);
			});
			m_programmaticClickOfUpdateGroups = false;
			SortByColumn(m_sortedColumn, m_sortedAscending);

			if (m_actorAssignmentViewModel.CharacterGroups.Any())
			{
				if (idOfSelectedGroup != null)
				{
					var groupToSelect = m_actorAssignmentViewModel.CharacterGroups.IndexOf(g => g.GroupId == idOfSelectedGroup);
					if (groupToSelect < 0)
						groupToSelect = 0;
					if (!m_characterGroupGrid.Rows[groupToSelect].Selected)
					{
						m_characterGroupGrid.ClearSelection();
						m_characterGroupGrid.Rows[groupToSelect].Selected = true;
					}
				}
			}
		}

		private void VoiceActorAssignmentDlg_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (m_characterGroupGrid.IsCurrentCellInEditMode)
				m_characterGroupGrid.EndEdit();

			if (m_project.IsVoiceActorAssignmentsComplete)
			{
				var unusedActors = m_project.UnusedActors.Count();
				if (unusedActors > 0)
				{
					string warningMsg;
					if (unusedActors == 1)
					{
						warningMsg = LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.UnusedActorWarningPlural",
							"The Voice Actor List for this project has 1 actor not assigned to any group.");
					}
					else
					{
						warningMsg =
							Format(LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.UnusedActorWarningPlural",
								"The Voice Actor List for this project has {0} actors not assigned to any group.", "{0} is a number."),
								unusedActors);
					}
					var msg =
						Format(LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.UnusedActorWarningInstructions",
							"{0} If this was not intentional," +
							" you can do either (or both) of the following:\r\n" +
							"1) In the Voice Actor List dialog box, delete or mark as inactive any unsed actors.\r\n" +
							"2) Change the list of groups of character roles by adjusting the cast size to be based on the actual Voice Actor List.\r\n\r\n" +
							"Would you like to take care of this now?", "{0} is the actual warning message."), warningMsg);
					e.Cancel = MessageBox.Show(this, msg, Text, MessageBoxButtons.YesNo) == DialogResult.Yes;
				}
			}
		}

		#region Character Group Grid
		private void HandleModelSaved(VoiceActorAssignmentViewModel sender, IEnumerable<CharacterGroup> changedGroups, bool requiresActorListRefresh)
		{
			m_selectingInResponseToDataChange = true;

			int columnIndex = m_characterGroupGrid.CurrentCellAddress.X;

			m_saveStatus.OnSaved();

			if (requiresActorListRefresh)
			{
				VoiceActorCol.DataSource = GetMultiColumnActorDataTable(null);
				SetVoiceActorCellDataSource();
			}

			if (!SetRowCount(changedGroups != null))
				InvalidateRowsForGroups(changedGroups);
			m_undoButton.Enabled = m_actorAssignmentViewModel.UndoActions.Any();
			if (!m_undoButton.Enabled)
				SetUndoOrRedoButtonToolTip(m_undoButton, null);
			m_redoButton.Enabled = m_actorAssignmentViewModel.RedoActions.Any();
			if (!m_redoButton.Enabled)
				SetUndoOrRedoButtonToolTip(m_redoButton, null);

			var groupToSelect = changedGroups == null ? null : changedGroups.LastOrDefault();
			int rowIndexOfGroupToSelect = groupToSelect == null ? -1 : m_actorAssignmentViewModel.CharacterGroups.IndexOf(groupToSelect);
			if (rowIndexOfGroupToSelect >= 0)
				m_characterGroupGrid.CurrentCell = m_characterGroupGrid.Rows[rowIndexOfGroupToSelect].Cells[columnIndex];

			m_selectingInResponseToDataChange = false;
		}

		private void InvalidateRowsForGroups(IEnumerable<CharacterGroup> changedGroups)
		{
			foreach (var group in changedGroups)
			{
				var i = m_actorAssignmentViewModel.CharacterGroups.IndexOf(@group);
				if (i >= 0)
					m_characterGroupGrid.InvalidateRow(i);
			}
		}

		private void m_characterGroupGrid_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyData == Keys.Delete)
			{
				bool multipleGroupsSelected = m_characterGroupGrid.SelectedRows.Count > 1;

				string dlgMessage = multipleGroupsSelected
					? LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.ConfirmUnassignDialog.MessagePlural",
						"Are you sure you want to remove the voice actor assignments from the selected character groups?")
					: LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.ConfirmUnassignDialog.MessageSingular",
						"Are you sure you want to remove the voice actor assignment from the selected character group?");
				string dlgTitle = LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.ConfirmUnassignDialog.Title", "Confirm");

				if (MessageBox.Show(this, dlgMessage, dlgTitle, MessageBoxButtons.YesNo) == DialogResult.Yes)
					UnAssignActorsFromSelectedGroups();
			}
		}

		private void HandleGridCellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
		{
			var grid = (DataGridView)sender;
			if (e.RowIndex < 0 || e.Button != MouseButtons.Right ||
				grid.SelectedRows.Contains(grid.Rows[e.RowIndex]))
				return;

			var column = e.ColumnIndex;
			if (grid.Columns[e.ColumnIndex] == VoiceActorCol)
				column = AttributesCol.Index;
			grid.CurrentCell = grid[column, e.RowIndex];
		}

		private void m_characterGroupGrid_DragOver(object sender, DragEventArgs e)
		{
			Point p = m_characterGroupGrid.PointToClient(new Point(e.X, e.Y));
			var hitInfo = m_characterGroupGrid.HitTest(p.X, p.Y);
			if (hitInfo.Type == DataGridViewHitTestType.Cell && e.Data.GetDataPresent(typeof(List<string>)) &&
				m_characterGroupGrid.Columns[hitInfo.ColumnIndex].Name == CharacterIdsCol.Name)
			{
				if (m_actorAssignmentViewModel.CanMoveCharactersToGroup((List<string>) e.Data.GetData(typeof (List<string>)), null))
				{
					//Follow the status of the drag-n-drop to remove new row on completion
					if (m_characterGroupGrid.EditingControl != null)
					{
						m_characterGroupGrid.EditingControl.QueryContinueDrag -= m_characterGroupGrid_DragEnd_DisallowUserToAddRows;
						if (!m_characterGroupGrid.Rows[hitInfo.RowIndex].IsNewRow)
							m_characterGroupGrid.EditingControl.QueryContinueDrag += m_characterGroupGrid_DragEnd_DisallowUserToAddRows;
					}

					m_characterGroupGrid.AllowUserToAddRows = true;
				}

				e.Effect = DragDropEffects.Move;
				return;
			}
			e.Effect = DragDropEffects.None;
		}

		private void m_characterGroupGrid_DragEnd_DisallowUserToAddRows(object sender, QueryContinueDragEventArgs e)
		{
			if (e.Action == DragAction.Cancel || e.Action == DragAction.Drop)
				m_characterGroupGrid.AllowUserToAddRows = false;
		}

		private void HandleCharacterIdDrop(List<string> localizedCharacterIds, int rowIndex)
		{
			IList<string> characterIds = new List<string>(localizedCharacterIds.Count);
			foreach (var localizedCharacterId in localizedCharacterIds)
				characterIds.Add(CharacterVerseData.SingletonLocalizedCharacterIdToCharacterIdDictionary[localizedCharacterId]);

			CharacterGroup dropGroup = (m_characterGroupGrid.Rows[rowIndex].IsNewRow) ? null :
				m_actorAssignmentViewModel.CharacterGroups[rowIndex];

			// Do this before creating the new group because it causes RowCount to go back down to reflect the number of non-empty rows.
			m_characterGroupGrid.AllowUserToAddRows = false;

			m_actorAssignmentViewModel.MoveCharactersToGroup(characterIds.ToReadOnlyList(), dropGroup, ConfirmCriticalProximityDegradation);
		}

		private void m_characterGroupGrid_DragDrop(object sender, DragEventArgs e)
		{
			Point p = m_characterGroupGrid.PointToClient(new Point(e.X, e.Y));
			var hitInfo = m_characterGroupGrid.HitTest(p.X, p.Y);
			if (hitInfo.Type == DataGridViewHitTestType.Cell)
			{
				if (e.Data.GetDataPresent(typeof(List<string>)) && m_characterGroupGrid.Columns[hitInfo.ColumnIndex].Name == CharacterIdsCol.Name)
				{
					List<string> characterIds = e.Data.GetData(typeof(List<string>)) as List<string>;
					if (characterIds != null)
						HandleCharacterIdDrop(characterIds, hitInfo.RowIndex);
				}
			}
		}

		//private void m_characterGroupGrid_MouseMove(object sender, MouseEventArgs e)
		//{
		//	if (e.Button == MouseButtons.Left)
		//	{
		//		var hitInfo = m_characterGroupGrid.HitTest(e.X, e.Y);
		//		if (hitInfo.Type == DataGridViewHitTestType.Cell && m_characterGroupGrid.Columns[hitInfo.ColumnIndex].Name == CharacterIdsCol.Name)
		//		{
		//			//Although the DataGridViewListBox usually handles the drag and drop, it only does so when the cell has focus.
		//			//In this case, the user starts dragging the first character ID even before selecting the row
		//			var group = m_actorAssignmentViewModel.CharacterGroups[hitInfo.RowIndex];
		//			if (group == null)
		//				return;
		//			string characterId = group.CharacterIds.ToList().FirstOrDefault();
		//			if (group.CharacterIds.Count == 1 && characterId != null)
		//			{
		//				//Without refreshing, the rows selected displays weirdly
		//				Refresh();
		//				DoDragDrop(new List<string> { characterId }, DragDropEffects.Move);
		//			}
		//		}
		//	}
		//}

		private void m_characterGroupGrid_CellEnter(object sender, DataGridViewCellEventArgs e)
		{
			if (m_characterGroupGrid.CurrentCell.EditType == typeof(DataGridViewMultiColumnComboBoxEditingControl))
			{
				SetVoiceActorCellDataSource();
				if (!m_selectingInResponseToDataChange)
					SendKeys.Send("{F4}");
			}
		}

		private void m_characterGroupGrid_CellLeave(object sender, DataGridViewCellEventArgs e)
		{
			//ResetVoiceActorCellDataSource();
		}

		private void m_characterGroupGrid_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
		{
			if (e.RowIndex >= m_actorAssignmentViewModel.CharacterGroups.Count)
				return;
			var group = m_actorAssignmentViewModel.CharacterGroups[e.RowIndex];
			var isCameo = group != null && group.AssignedToCameoActor;
			// Need to check before setting to avoid making it impossible to open the drop-down list.
			if (m_characterGroupGrid.Rows[e.RowIndex].Cells[VoiceActorCol.Name].ReadOnly != isCameo)
				m_characterGroupGrid.Rows[e.RowIndex].Cells[VoiceActorCol.Name].ReadOnly = isCameo;
			if (isCameo)
			{
				var charIdsString = m_actorAssignmentViewModel.CharacterGroups[e.RowIndex].CharacterIds.ToString();
				if (IsNullOrWhiteSpace(charIdsString) && !(m_characterGroupGrid[CharacterIdsCol.Index, e.RowIndex] is DataGridViewLinkCell))
					m_characterGroupGrid[CharacterIdsCol.Index, e.RowIndex] = new DataGridViewLinkCell();

				string actorIsCameo = LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.CameoTooltip",
					"This actor is assigned to perform a cameo role.");
				if (!Char.IsWhiteSpace(actorIsCameo.Last()))
					actorIsCameo += " ";

				if (group.CharacterIds.Any())
				{
					m_characterGroupGrid.Rows[e.RowIndex].Cells[VoiceActorCol.Name].ToolTipText =
						actorIsCameo +
						LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.CameoChangeCharactersTooltip",
							"You can change the characters in this group, but you cannot change the actor assignment.");
				}
				else
				{
					m_characterGroupGrid.Rows[e.RowIndex].Cells[VoiceActorCol.Name].ToolTipText = actorIsCameo +
						LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.CameoAddCharactersTooltip",
							"You can add characters to this group, but you cannot change the actor assignment.");
				}
			}
			else
				m_characterGroupGrid.Rows[e.RowIndex].Cells[VoiceActorCol.Name].ToolTipText = "";
		}

		private void m_characterGroupGrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
		{
			if (e.RowIndex >= m_actorAssignmentViewModel.CharacterGroups.Count || e.ColumnIndex != CharacterIdsCol.Index)
				return;
			var group = m_actorAssignmentViewModel.CharacterGroups[e.RowIndex];
			if (group != null && group.AssignedToCameoActor)
			{
				if (!group.CharacterIds.Any())
				{
					//var cellBounds = m_characterGroupGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].GetContentBounds(e.RowIndex);
					//e.CellStyle.ForeColor = cellBounds.Contains(this.PointToClient(Control.MousePosition))
					//	? SystemColors.HotTrack : SystemColors.Highlight;
					e.CellStyle.ForeColor = GlyssenColorPalette.ColorScheme.LinkColor;
					e.CellStyle.Font = m_hyperlinkFont;
				}
			}
		}

		private void m_characterGroupGrid_DataError(object sender, DataGridViewDataErrorEventArgs e)
		{
			var src = (DataGridView)sender;
			// ignore most ArgumentExceptions in the VoiceActorCol
			// this can happen if you type in the name of an existing cameo actor
			if (e.Exception is ArgumentException)
			{
				if (src.CurrentCell.OwningColumn == VoiceActorCol)
				{
					var success = int.TryParse(src.CurrentCell.Value.ToString(), out var currentVal);
					if (success && currentVal > -1)
					{
						e.Cancel = true;
						return;
					}
				}
			}

			Analytics.ReportException(e.Exception);
			Logger.WriteEvent($"Following error is fatal exception from {src.Name}. Row: {e.RowIndex}; Column: {e.ColumnIndex}");
			ErrorReport.ReportFatalException(e.Exception);
			throw e.Exception;
		}

		private void m_characterGroupGrid_SelectionChanged(object sender, EventArgs e)
		{
			bool exactlyOneGroupSelected = m_characterGroupGrid.SelectedRows.Count == 1 &&
				m_characterGroupGrid.SelectedRows[0].Index < m_actorAssignmentViewModel.CharacterGroups.Count;

			m_splitSelectedGroupButton.Enabled = exactlyOneGroupSelected && FirstSelectedCharacterGroup.CharacterIds.Count > 1;
			m_toolStripButtonFindNextMatchingCharacter.Enabled = m_toolStripTextBoxFindCharacter.TextLength > 0;

			m_characterIdsForSelectedGroup = new List<string>();

			if (exactlyOneGroupSelected)
			{
				UpdateDisplayForSingleCharacterGroupSelected();
			}
			else
			{
				m_characterDetailsGrid.Visible = false;
				m_linkLabelShowHideDetails.Visible = false;
				m_btnMove.Enabled = false;
			}
			if (!m_tableLayoutPanelMove.Visible)
			{
				ShowOrHideCharacterDetails(false);
			}
		}

		private void UpdateDisplayForSingleCharacterGroupSelected()
		{
			m_characterDetailsGrid.Visible = true;
			var currentGroup = FirstSelectedCharacterGroup;
			if (currentGroup.CharacterIds.Any())
			{
				m_characterIdsForSelectedGroup = currentGroup.CharacterIds.ToList();
				m_characterDetailsGrid.Visible = m_characterDetailsVisible;
				m_linkLabelShowHideDetails.Visible = true;
				m_characterDetailsGrid.RowCount = m_characterIdsForSelectedGroup.Count;
				// ENHANCE: For groups with lots of characters, this Refresh can be very slow and cause the UI to appear to hang. Can it be
				// done on a background thread? Or more likely the details grid should be virtual.
				m_characterDetailsGrid.Refresh();
				//m_lblNoCharactersInGroup.Visible = false;
				//m_lblHowToAssignCharactersToCameoGroup.Visible = false;
				m_btnMove.Enabled = m_pendingMoveCharacters != null && !currentGroup.CharacterIds.Contains(m_pendingMoveCharacters.First());
			}
			else 
			{
				//m_characterDetailsGrid.Visible = false;
				//if (FirstSelectedCharacterGroup.AssignedToCameoActor)
				//{
				//	//m_lblHowToAssignCharactersToCameoGroup.Visible = true;
				//	//m_lblNoCharactersInGroup.Visible = false;
				//	m_lblHowToAssignCharactersToCameoGroup.Text = string.Format(m_fmtNoCharactersInGroup,
				//		m_lblNoCharactersInGroup.Text, m_project.VoiceActorList.GetVoiceActorById(currentGroup.VoiceActorId).Name);
				//}
				////else
				////{
				//	//m_lblHowToAssignCharactersToCameoGroup.Visible = false;
				//	//m_lblNoCharactersInGroup.Visible = true;
				////}
				m_btnMove.Enabled = m_pendingMoveCharacters != null;
			}
			m_linkLabelShowHideDetails.Text = Format(m_characterDetailsVisible ? m_fmtHideCharacterDetails : m_fmtShowCharacterDetails, currentGroup.GroupIdForUiDisplay);
		}

		private void m_characterGroupGrid_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
		{
			if (m_actorAssignmentViewModel.CharacterGroups.Count < e.RowIndex + 1)
			{
				e.Value = null;
				return;
			}
			if (e.ColumnIndex == GroupIdCol.Index)
				e.Value = m_actorAssignmentViewModel.CharacterGroups[e.RowIndex].GroupIdForUiDisplay;
			else if (e.ColumnIndex == CharacterIdsCol.Index)
			{
				var charIdsString = m_actorAssignmentViewModel.CharacterGroups[e.RowIndex].CharacterIds.ToString();
				e.Value = IsNullOrWhiteSpace(charIdsString) ? LocalizationManager.GetString(
					"DialogBoxes.VoiceActorAssignmentDlg.SelectCameoRoleLink", "Select a cameo role",
					"Displayed as link in the \"Characters In Group\" column when the group assigned to a cameo actor has no characters in it.")
					: charIdsString;
			}
			else if (e.ColumnIndex == AttributesCol.Index)
				e.Value = m_actorAssignmentViewModel.CharacterGroups[e.RowIndex].AttributesDisplay;
			else if (e.ColumnIndex == CharStatusCol.Index)
				e.Value = m_actorAssignmentViewModel.CharacterGroups[e.RowIndex].StatusDisplay;
			else if (e.ColumnIndex == EstimatedHoursCol.Index)
				e.Value = m_actorAssignmentViewModel.CharacterGroups[e.RowIndex].EstimatedHours;
			else if (e.ColumnIndex == VoiceActorCol.Index)
				e.Value = m_actorAssignmentViewModel.CharacterGroups[e.RowIndex].VoiceActorId;
		}

		private void m_characterGroupGrid_CellValuePushed(object sender, DataGridViewCellValueEventArgs e)
		{
			if (e.ColumnIndex == VoiceActorCol.Index)
				m_actorAssignmentViewModel.AssignActorToGroup((int) e.Value, m_actorAssignmentViewModel.CharacterGroups[e.RowIndex]);
		}

		private void HandleGridColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left)
				return;
			var clickedColumn = m_characterGroupGrid.Columns[e.ColumnIndex];
			if (clickedColumn.SortMode == DataGridViewColumnSortMode.NotSortable)
				return;

			// We generally want to sort ascending unless it already was ascending, but for estimated
			// hours, we want descending to be the default.
			var sortAscending = (clickedColumn == EstimatedHoursCol) ? (clickedColumn.HeaderCell.SortGlyphDirection == SortOrder.Descending) :
				(clickedColumn.HeaderCell.SortGlyphDirection != SortOrder.Ascending);

			foreach (var column in m_characterGroupGrid.Columns.OfType<DataGridViewColumn>())
			{
				if (column != clickedColumn)
					column.HeaderCell.SortGlyphDirection = SortOrder.None;
			}

			SortByColumn(clickedColumn, sortAscending);
		}

		private void SetVoiceActorCellDataSource()
		{
			if (m_characterGroupGrid.CurrentRow == null || m_characterGroupGrid.CurrentCell.EditType != typeof(DataGridViewMultiColumnComboBoxEditingControl) ||
				m_characterGroupGrid.CurrentCellAddress.Y >= m_actorAssignmentViewModel.CharacterGroups.Count)
				return;
			var group = m_actorAssignmentViewModel.CharacterGroups[m_characterGroupGrid.CurrentCellAddress.Y];
			var cell = (DataGridViewComboBoxCell)m_characterGroupGrid.CurrentCell;
			cell.DataSource = GetMultiColumnActorDataTable(group);
		}

		private void ResetVoiceActorCellDataSource()
		{
			if (m_characterGroupGrid.CurrentCell == null)
				return;
			if (m_characterGroupGrid.CurrentCell.EditType == typeof(DataGridViewMultiColumnComboBoxEditingControl))
			{
				var cell = m_characterGroupGrid.CurrentCell as DataGridViewComboBoxCell;
				if (cell == null)
					return;
				cell.DataSource = GetMultiColumnActorDataTable(null);
			}
		}

		private void SortByColumn(DataGridViewColumn column, bool sortAscending)
		{
			ResetVoiceActorCellDataSource();

			column.HeaderCell.SortGlyphDirection = sortAscending ? SortOrder.Ascending : SortOrder.Descending;

			if (column == AttributesCol)
				m_actorAssignmentViewModel.Sort(SortedBy.Attributes, sortAscending);
			else if (column == EstimatedHoursCol)
				m_actorAssignmentViewModel.Sort(SortedBy.EstimatedTime, sortAscending);
			else if (column == VoiceActorCol)
				m_actorAssignmentViewModel.Sort(SortedBy.Actor, sortAscending);
			else
				m_actorAssignmentViewModel.Sort(SortedBy.Name, sortAscending);

			m_sortedColumn = column;
			m_sortedAscending = sortAscending;
			m_characterGroupGrid.Refresh();

			SetVoiceActorCellDataSource();
		}
		#endregion

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			var gridSettings = Settings.Default.CharacterGroupGridGridSettings;

			if (gridSettings != null && !IsNullOrEmpty(gridSettings.SortedColumn) && m_characterGroupGrid.Columns.Contains(gridSettings.SortedColumn))
				SortByColumn(m_characterGroupGrid.Columns[gridSettings.SortedColumn], gridSettings.SortDirection == SortOrder.Ascending.ToString());
			else
				SortByColumn(EstimatedHoursCol, false);
			SetRowCount();
			if (m_actorAssignmentViewModel.CharacterGroups.Any()) // This should always be true, but just to be sure.
				m_characterGroupGrid.Rows[0].Selected = true;

			var detailsRowStyle = m_tableLayoutPanel.LayoutSettings.RowStyles[m_tableLayoutPanel.GetRow(m_characterDetailsGrid)];
			var groupsRowStyle = m_tableLayoutPanel.LayoutSettings.RowStyles[m_tableLayoutPanel.GetRow(m_characterGroupGrid)];
			detailsRowStyle.Height = 0;
			groupsRowStyle.Height = 100;
		}

		protected override void OnDeactivate(EventArgs e)
		{
			base.OnDeactivate(e);
			if (m_characterGroupGrid.IsCurrentCellInEditMode)
				m_characterGroupGrid.EndEdit();
		}

		private void HandleUndoButtonClick(object sender, EventArgs e)
		{
			m_selectingInResponseToDataChange = true;
			try
			{
				if (!m_actorAssignmentViewModel.Undo())
					MessageBox.Show(this, LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.UndoFailed", "Undo Failed!"));
			}
			finally
			{
				m_selectingInResponseToDataChange = false;
			}
			// TODO: It would be nice to be able to immediately change the tool tip text for the button, but it would take some extra work.
		}

		private void HandleRedoButtonClick(object sender, EventArgs e)
		{
			m_selectingInResponseToDataChange = true;
			try
			{
				if (!m_actorAssignmentViewModel.Redo())
					MessageBox.Show(this, LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.RedoFailed", "Redo Failed!"));
			}
			finally
			{
				m_selectingInResponseToDataChange = false;
			}
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.Control)
			{
				foreach (var item in m_toolStrip.Items.OfType<ToolStripItem>().Where(i => i.Tag is Tuple<string, Keys>))
				{
					if (e.KeyCode == ((Tuple<string, Keys>)item.Tag).Item2)
					{
						item.PerformClick();
						return;
					}
				}
			}
			base.OnKeyDown(e);
		}

		private void SetUndoOrRedoButtonToolTip(object sender, EventArgs e)
		{
			var btn = (ToolStripButton)sender;
			var tag = (Tuple<string, Keys>)btn.Tag;
			string ctrlKeyTip = Format("(Ctrl-{0})", tag.Item2);
			var description = (btn == m_undoButton ? m_actorAssignmentViewModel.UndoActions : m_actorAssignmentViewModel.RedoActions).FirstOrDefault();
			if (description == null)
				description = Empty;
			else if (!description.EndsWith(" "))
				description += " ";
			btn.ToolTipText = Format(tag.Item1, description + ctrlKeyTip);
		}

		private void m_characterGroupGrid_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
		{
			var dropDown = e.Control as DataGridViewComboBoxEditingControl;
			if (dropDown == null)
				return;
			dropDown.DropDownStyle = ComboBoxStyle.DropDown;
			dropDown.AutoCompleteMode = AutoCompleteMode.None;
			dropDown.DropDownClosed += DropDownOnDropDownClosed;
		}

		private void m_characterGroupGrid_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
		{
			if (e.ColumnIndex == VoiceActorCol.DisplayIndex)
				SaveActorAssignment(e.FormattedValue.ToString(), e.RowIndex);
		}

		private void SaveActorAssignment(string formattedValue, int rowIndex)
		{
			int value;
			if (!DataTableTryGetValueForDisplayMember(VoiceActorCol, formattedValue, out value))
			{
				m_actorAssignmentViewModel.AddNewActorToGroup(formattedValue, FirstSelectedCharacterGroup);
			}
			else
			{
				m_characterGroupGrid[VoiceActorCol.DisplayIndex, rowIndex].Value = value;
			}
			SetVoiceActorCellDataSource();
			VoiceActorCol.DataSource = GetMultiColumnActorDataTable(null);
		}

		private bool DataTableTryGetValueForDisplayMember(DataGridViewMultiColumnComboBoxColumn column, string formattedValue, out int value)
		{
			value = -1;
			var dataTable = column.DataSource as DataTable;
			if (dataTable == null)
				return false;
			var displayMember = column.DisplayMember;
			var valueMember = column.ValueMember;
			if (!IsNullOrEmpty(displayMember) && !IsNullOrEmpty(valueMember) && dataTable.Rows.Count > 0)
			{
				for (int i = 0; i < dataTable.Rows.Count; i++)
				{
					var row = dataTable.Rows[i];
					if (row[displayMember].Equals(formattedValue))
					{
						value = (int)row[valueMember];
						return true;
					}
				}
			}
			return false;
		}

		public DataTable GetMultiColumnActorDataTable(CharacterGroup group)
		{
			var table = new DataTable();
			table.Columns.Add("ID", typeof(int));
			table.Columns.Add("Category");
			table.Columns.Add("Icon", typeof(Image));
			table.Columns.Add("Name");
			table.Columns.Add("Gender");
			table.Columns.Add("Age");
			table.Columns.Add("Cameo");
			table.Columns.Add("SpecialUse");

			string category = LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.Categories.AvailableVoiceActors", "Available:");
			bool rowForRemoveActorAdded = false;
			foreach (Tuple<VoiceActor, bool> actorInfo in m_actorAssignmentViewModel.GetActorsSortedByAvailabilityAndName(group))
			{
				if (!actorInfo.Item2 && !rowForRemoveActorAdded)
				{
					AddRowForRemoveActor(table);
					category = LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.Categories.AlreadyAssignedVoiceActors",
						"Assigned to a Character Group:");
					rowForRemoveActorAdded = true;
				}

				table.Rows.Add(GetDataTableRow(actorInfo.Item1, category));
			}
			if (!rowForRemoveActorAdded)
				AddRowForRemoveActor(table);

			return table;
		}

		private static void AddRowForRemoveActor(DataTable table)
		{
			table.Rows.Add(
				-1,
				null,
				Resources.RemoveActor,
				"",
				//LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.RemoveVoiceActorAssignment", "Remove Voice Actor Assignment"),
				"",
				"",
				"",
				LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.RemoveVoiceActorAssignment", "Remove Voice Actor Assignment"));
		}

		private object[] GetDataTableRow(VoiceActor actor, string category)
		{
			return new object[]
			{
				actor.Id,
				category,
				null,
				actor.Name,
				GetUiStringForActorGender(actor.Gender),
				GetUiStringForActorAge(actor.Age),
				actor.IsCameo ? LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.Cameo", "Cameo") : "",
				null
			};
		}

		private static string GetUiStringForActorGender(ActorGender actorGender)
		{
			switch (actorGender)
			{
				case ActorGender.Male: return LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.ActorGender.Male", "Male");
				case ActorGender.Female: return LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.ActorGender.Female", "Female");
				default: return string.Empty;
			}
		}

		private static string GetUiStringForActorAge(ActorAge actorAge)
		{
			switch (actorAge)
			{
				case ActorAge.Adult: return LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.CharacterAge.Adult", "Adult");
				case ActorAge.Child: return LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.CharacterAge.Child", "Child");
				case ActorAge.Elder: return LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.CharacterAge.Elder", "Elder");
				case ActorAge.YoungAdult: return LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.CharacterAge.YoungAdult", "Young Adult");
				default: return string.Empty;
			}
		}

		/// <summary>
		/// When the drop-down closes, we want to force the change to be committed and get out of edit mode. This gives a better user experience,
		/// especially when reassigning an actor to a different group (otherwise, the actor looks like he's assigned to both groups).
		/// </summary>
		private void DropDownOnDropDownClosed(object sender, EventArgs eventArgs)
		{
			var dropDown = sender as DataGridViewComboBoxEditingControl;
			if (dropDown == null)
				return;

			// If there was no actual change, the view model correctly ignores it.
			m_characterGroupGrid.NotifyCurrentCellDirty(true);
			if (m_characterGroupGrid.IsCurrentCellInEditMode)
			{
				SaveActorAssignment(m_characterGroupGrid.CurrentCell.EditedFormattedValue.ToString(), m_characterGroupGrid.CurrentCellAddress.Y);
				m_characterGroupGrid.EndEdit(DataGridViewDataErrorContexts.Commit);
			}

			dropDown.DropDownClosed -= DropDownOnDropDownClosed;
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			switch (keyData)
			{
				case Keys.Escape:
					// Escape will result in the drop down closing, but the current editing operation needs
					// to be abandoned and we need to prevent the subsequent attempt to commit the edit (which
					// would actually result in a crash).
					var comboBox = m_characterGroupGrid.EditingControl as DataGridViewComboBoxEditingControl;
					if (comboBox != null)
					{
						m_characterGroupGrid.NotifyCurrentCellDirty(false);
						comboBox.DropDownClosed -= DropDownOnDropDownClosed;
						comboBox.DroppedDown = false;
					}
					break;
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}

		private void m_linkClose_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}

		#region Events related to the Character Details grid
		private void m_characterDetailsGrid_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
		{
			if (e.RowIndex < 0 || m_characterIdsForSelectedGroup == null || m_characterIdsForSelectedGroup.Count <= e.RowIndex)
				return;

			var characterId = m_characterIdsForSelectedGroup[e.RowIndex];

			if (e.ColumnIndex == CharacterDetailsIdCol.Index)
				e.Value = characterId;
			else if (e.ColumnIndex == CharacterDetailsGenderCol.Index)
				e.Value = m_actorAssignmentViewModel.GetUiStringForCharacterGender(characterId);
			else if (e.ColumnIndex == CharacterDetailsAgeCol.Index)
				e.Value = m_actorAssignmentViewModel.GetUiStringForCharacterAge(characterId);
			else if (e.ColumnIndex == CharacterDetailsHoursCol.Index)
				e.Value = m_actorAssignmentViewModel.GetEstimatedHoursForCharacter(characterId);
		}

		private void HandleShowOrHideCharacterDetails_Click(object sender, LinkLabelLinkClickedEventArgs e)
		{
			m_detailPinned = !m_characterDetailsVisible;
			ShowOrHideCharacterDetails(m_detailPinned);
		}

		private void ShowOrHideCharacterDetails(bool show)
		{
			if (!show && m_detailPinned)
				return;

			if (m_characterDetailsVisible == show)
				return;

			m_characterDetailsVisible = show;
			if (m_characterDetailsVisible)
			{
				SetCharacterDetailsPanePercentage();

				UpdateDisplayForSingleCharacterGroupSelected();
			}
			else
			{
				int maxHeight = m_characterGroupGrid.Height + m_characterDetailsGrid.Height;
				int minHeight = m_characterGroupGrid.Height;
				var fullHeight = m_characterGroupGrid.Rows.GetRowsHeight(DataGridViewElementStates.None) +
								m_characterGroupGrid.ColumnHeadersHeight + 2;
				if (fullHeight > minHeight)
					m_characterGroupGrid.Height = Math.Min(fullHeight, maxHeight);

				var detailsRowStyle = m_tableLayoutPanel.LayoutSettings.RowStyles[m_tableLayoutPanel.GetRow(m_characterDetailsGrid)];
				var groupsRowStyle = m_tableLayoutPanel.LayoutSettings.RowStyles[m_tableLayoutPanel.GetRow(m_characterGroupGrid)];
				detailsRowStyle.Height = 0;
				groupsRowStyle.Height = 100;
				if (FirstSelectedCharacterGroup != null)
					m_linkLabelShowHideDetails.Text = Format(m_fmtShowCharacterDetails, FirstSelectedCharacterGroup.GroupIdForUiDisplay);
				else
					m_linkLabelShowHideDetails.Text = Empty;
			}
			m_characterDetailsGrid.Visible = m_characterDetailsVisible;
		}

		private void SetCharacterDetailsPanePercentage()
		{
			var groupsRowStyle = m_tableLayoutPanel.LayoutSettings.RowStyles[m_tableLayoutPanel.GetRow(m_characterGroupGrid)];
			var detailsRowStyle = m_tableLayoutPanel.LayoutSettings.RowStyles[m_tableLayoutPanel.GetRow(m_characterDetailsGrid)];
			//int groupCount = m_actorAssignmentViewModel.CharacterGroups.Count;

			//TODO: put this back to being a calculation
			// This line was throwing an exception sometimes. Do not restore without determining the problem.
			//double percentage = (double)groupCount / (groupCount + m_actorAssignmentViewModel.CharacterGroups.Max(g => g.CharacterIds.Count));

			groupsRowStyle.Height = 70;//Math.Min(Math.Max((int)((1 - percentage) * 100), 45), 85);
			detailsRowStyle.Height = 30;//100 - groupsRowStyle.Height;
		}
		#endregion

		#region Find a character
		private void m_toolStripTextBoxFindCharacter_TextChanged(object sender, EventArgs e)
		{
			m_currentMatches = null;
			InitiateFind();
		}

		private void InitiateFind()
		{
			m_toolStripButtonFindNextMatchingCharacter.Enabled = false;
			if (m_toolStripTextBoxFindCharacter.Text.Trim().Length < 2)
			{
				if (m_findCharacterBackgroundWorker.IsBusy && !m_findCharacterBackgroundWorker.CancellationPending)
					m_findCharacterBackgroundWorker.CancelAsync();
				if (m_determineMatchingCharactersBackgroundWorker.IsBusy && !m_determineMatchingCharactersBackgroundWorker.CancellationPending)
					m_determineMatchingCharactersBackgroundWorker.CancelAsync();
				return;
			}

			if (m_findCharacterBackgroundWorker.IsBusy)
			{
				if (!m_findCharacterBackgroundWorker.CancellationPending)
					m_findCharacterBackgroundWorker.CancelAsync();
				return;
			}
			object[] parameters = { m_toolStripTextBoxFindCharacter.Text, m_characterGroupGrid.CurrentCellAddress.Y,
				m_characterDetailsGrid.Visible && m_characterDetailsGrid.RowCount > 0 ? m_characterDetailsGrid.CurrentCellAddress.Y : 0 };
			
			m_findCharacterBackgroundWorker.RunWorkerAsync(parameters);
		}

		private void FindCharacter(object sender, DoWorkEventArgs e)
		{
			var parameters = (object[])e.Argument;
			var textToFind = (string)parameters[0];
			var startingGroupRow = (int)parameters[1];
			var startingDetailRow = (int)parameters[2];
			e.Result = m_actorAssignmentViewModel.FindNextMatchingCharacter(textToFind, startingGroupRow, startingDetailRow);
		}

		private void FindCharacterCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Cancelled)
			{
				m_currentMatches = null;
				InitiateFind();
				return;
			}

			var match = (Tuple<int, int>)e.Result;
			var characterGroupIndex = match.Item1;
			var characterDetailIndex = match.Item2;

			if (characterGroupIndex < 0 || characterDetailIndex < 0)
			{
				// No matches.
				m_lblMatches.ForeColor = Color.Red;
				m_lblMatches.Text = LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.NoMatchingCharacters", "no matches");
				SystemSounds.Beep.Play();
				return;
			}

			if (m_currentMatches == null)
			{
				bool cancelling = false;
				if (m_determineMatchingCharactersBackgroundWorker.IsBusy)
				{
					cancelling = true;
					if (!m_determineMatchingCharactersBackgroundWorker.CancellationPending)
						m_determineMatchingCharactersBackgroundWorker.CancelAsync();
				}

				if (!cancelling)
				{
					m_determineMatchingCharactersBackgroundWorker.RunWorkerAsync(m_toolStripTextBoxFindCharacter.Text);
				}
			}
			UpdateMatchLabelDisplay(match);

			if (m_characterGroupGrid.CurrentCellAddress.Y == characterGroupIndex && m_characterDetailsGrid.CurrentCellAddress.Y == characterDetailIndex)
			{
				// Search wrapped around.
				return;
			}

			m_characterGroupGrid.CurrentCell = m_characterGroupGrid.Rows[characterGroupIndex].Cells[CharacterIdsCol.Index];

			ShowOrHideCharacterDetails(true);

			foreach (DataGridViewRow row in m_characterDetailsGrid.Rows)
				row.Selected = row.Index == characterDetailIndex;
			if (m_characterDetailsGrid.CurrentCell == m_characterDetailsGrid.Rows[characterDetailIndex].Cells[CharacterDetailsIdCol.Index])
			{
				// HACK: If the current cell is already the one we want to display, setting CurrentCell will not scroll it into view
				m_characterDetailsGrid.CurrentCell = m_characterDetailsGrid.Rows[characterDetailIndex].Cells[CharacterDetailsGenderCol.Index];
			}
			m_characterDetailsGrid.CurrentCell = m_characterDetailsGrid.Rows[characterDetailIndex].Cells[CharacterDetailsIdCol.Index];
			m_toolStripButtonFindNextMatchingCharacter.Enabled = true;
		}

		private void GetInidicesOfMatchingCharacters(object sender, DoWorkEventArgs e)
		{
			var textToFind = (string)e.Argument;
			var matches = new SortedSet<Tuple<int, int>>(new IndexPairComparer());
			int groupIndex = -1;
			int characterIndex = -1;
			do
			{
				var matchingCharacter = m_actorAssignmentViewModel.FindNextMatchingCharacter(textToFind, groupIndex, characterIndex);
				if (matchingCharacter.Item1 == -1 || !matches.Add(matchingCharacter))
					break;
				groupIndex = matchingCharacter.Item1;
				characterIndex = matchingCharacter.Item2;
			} while (!e.Cancel);
			e.Result = matches;
		}

		private class IndexPairComparer : IComparer<Tuple<int, int>>
		{
			public int Compare(Tuple<int, int> x, Tuple<int, int> y)
			{
				var item1Comparison = x.Item1.CompareTo(y.Item1);
				if (item1Comparison != 0)
					return item1Comparison;
				return x.Item2.CompareTo(y.Item2);
			}
		}

		private void UpdateMatchLabelDisplay(Tuple<int, int> match = null)
		{
			if (m_currentMatches == null)
				m_lblMatches.Text = Empty;
			else
			{
				if (match == null)
				{
					// Use current location
					match = new Tuple<int, int>(m_characterGroupGrid.CurrentCellAddress.Y,
						m_characterDetailsGrid.Visible && m_characterDetailsGrid.RowCount > 0 ? m_characterDetailsGrid.CurrentCellAddress.Y : 0);
				}

				m_lblMatches.ForeColor = m_toolStripLabelFindCharacter.ForeColor;
				var i = m_currentMatches.IndexOf(match);
				m_lblMatches.Text = i >= 0 ? Format(m_fmtMatches, i + 1, m_currentMatches.Count) : Empty;
			}
		}

		private void GetCountOfMatchingCharactersCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Cancelled)
			{
				InitiateFind();
				return;
			}
			m_currentMatches = (SortedSet<Tuple<int, int>>)e.Result;
			UpdateMatchLabelDisplay();
		}

		private void m_toolStripButtonFindNextMatchingCharacter_Click(object sender, EventArgs e)
		{
			InitiateFind();
		}
		#endregion
	}
}
