using System;
using System.Collections.Generic;
using System.Windows.Forms;
using L10NSharp;
using L10NSharp.UI;
using L10NSharp.TMXUtils;

namespace Glyssen.Dialogs
{
	public partial class AddCharacterToGroupDlg : Form
	{
		private readonly AddCharactersToGroupViewModel m_viewModel;

		public AddCharacterToGroupDlg(AddCharactersToGroupViewModel model)
		{
			InitializeComponent();

			m_viewModel = model;
			HandleStringsLocalized();
			LocalizeItemDlg< TMXDocument>.StringsLocalized += HandleStringsLocalized;
			m_characterDetailsGrid.RowCount = m_viewModel.FilteredCharactersCount;
		}

		private void HandleStringsLocalized()
		{
			if (m_viewModel.AddingToCameoGroup)
			{
				Text = String.Format(LocalizationManager.GetString("DialogBoxes.SelectCameoRoleDlg.Title",
					"Select a Cameo Role for {0}"), m_viewModel.CameoActorName);
			}
		}

		public IList<string> SelectedCharacters { get; private set; }

		private void m_characterDetailsGrid_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
		{
			if (e.RowIndex < 0 || m_viewModel.FilteredCharactersCount <= e.RowIndex)
				return;

			if (e.ColumnIndex == CharacterDetailsIdCol.Index)
				e.Value = m_viewModel.GetLocalizedCharacterId(e.RowIndex);
			else if (e.ColumnIndex == CharacterDetailsGenderCol.Index)
				e.Value = m_viewModel.GetUiStringForCharacterGender(e.RowIndex);
			else if (e.ColumnIndex == CharacterDetailsAgeCol.Index)
				e.Value = m_viewModel.GetUiStringForCharacterAge(e.RowIndex);
			else if (e.ColumnIndex == CharacterDetailsHoursCol.Index)
				e.Value = m_viewModel.GetEstimatedHoursForCharacter(e.RowIndex);
		}

		private void m_btnOk_Click(object sender, EventArgs e)
		{
			SelectedCharacters = new List<string>();
			foreach (DataGridViewRow selectedRow in m_characterDetailsGrid.SelectedRows)
				SelectedCharacters.Add(m_viewModel.GetCharacterId(selectedRow.Index));
		}

		private void m_characterDetailsGrid_SelectionChanged(object sender, EventArgs e)
		{
			m_btnOk.Enabled = m_characterDetailsGrid.SelectedRows.Count > 0;
		}

		private void m_toolStripTextBoxFindCharacter_TextChanged(object sender, EventArgs e)
		{
			m_viewModel.FilterCharacterIds(m_toolStripTextBoxFindCharacter.Text);
			m_characterDetailsGrid.RowCount = m_viewModel.FilteredCharactersCount;
		}
	}
}
