using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Glyssen.Controls;
using GlyssenEngine.Script;
using static System.String;
using AssignCharacterViewModel = GlyssenEngine.ViewModels.AssignCharacterViewModel<System.Drawing.Font>;

namespace Glyssen.Dialogs
{
	public partial class AddCharacterDlg : Form
	{
		private readonly AssignCharacterViewModel m_viewModel;
		int m_characterListHoveredIndex = -1;
		private readonly ToolTip m_characterListToolTip = new ToolTip();

		public AddCharacterDlg(AssignCharacterViewModel viewModel)
		{
			InitializeComponent();
			m_viewModel = viewModel;
			ShowCharactersInBook();
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			m_txtCharacterFilter.Focus();
		}

		public AssignCharacterViewModel.Character SelectedCharacter =>
			m_listBoxCharacters.SelectedItem as AssignCharacterViewModel.Character;

		public AssignCharacterViewModel.Delivery SelectedDelivery =>
			m_listBoxDeliveries.SelectedItem as AssignCharacterViewModel.Delivery;

		private IEnumerable<AssignCharacterViewModel.Character> CurrentContextCharacters =>
			m_listBoxCharacters.Items.Cast<AssignCharacterViewModel.Character>();

		private bool IsCharacterAndDeliverySelectionComplete =>
			m_listBoxCharacters.SelectedIndex > -1 && m_listBoxDeliveries.SelectedIndex > -1;

		private bool IsDirty => m_viewModel.IsModified((AssignCharacterViewModel.Character)m_listBoxCharacters.SelectedItem,
			(AssignCharacterViewModel.Delivery)m_listBoxDeliveries.SelectedItem);

		private void m_icnDeliveryFilter_Click(object sender, EventArgs e)
		{
			m_txtDeliveryFilter.Focus();
		}

		private void m_txtDeliveryFilter_TextChanged(object sender, EventArgs e)
		{
			LoadDeliveryListBox(m_viewModel.GetUniqueDeliveries(m_txtDeliveryFilter.Text),
				(AssignCharacterViewModel.Delivery)m_listBoxDeliveries.SelectedItem);
		}

		private void m_btnAddCharacter_Click(object sender, EventArgs e)
		{
			AddNewCharacter(m_txtCharacterFilter.Text);
		}

		private void m_btnAddDelivery_Click(object sender, EventArgs e)
		{
			AddNewDelivery(m_txtDeliveryFilter.Text);
		}

		private void AddNewCharacter(string character)
		{
			if (IsNullOrWhiteSpace(character))
				return;

			var existingItem = CurrentContextCharacters.FirstOrDefault(c => c.ToString().Equals(character, StringComparison.OrdinalIgnoreCase));
			if (existingItem != null)
			{
				m_listBoxCharacters.SelectedItem = existingItem;
				return;
			}

			using (var dlg = new NewCharacterDlg(character))
			{
				MainForm.LogDialogDisplay(dlg);
				if (dlg.ShowDialog() != DialogResult.OK)
					return;

				m_viewModel.StoreCharacterDetail(character, dlg.Gender, dlg.Age);
			}

			var newItem = new AssignCharacterViewModel.Character(character, projectSpecific: true);
			m_listBoxCharacters.Items.Add(newItem);
			m_listBoxCharacters.SelectedItem = newItem;
		}

		private void AddNewDelivery(string delivery)
		{
			if (IsNullOrWhiteSpace(delivery))
				return;
			m_listBoxDeliveries.SelectedItem = m_listBoxDeliveries.Items.Cast<AssignCharacterViewModel.Delivery>()
				.FirstOrDefault(d => d.Text == delivery);
			if (m_listBoxDeliveries.SelectedItem != null)
				return;
			var newItem = new AssignCharacterViewModel.Delivery(delivery);
			m_listBoxDeliveries.Items.Add(newItem);
			m_listBoxDeliveries.SelectedItem = newItem;
		}

		private void m_icnCharacterFilter_Click(object sender, EventArgs e)
		{
			m_txtCharacterFilter.Focus();
		}

		private void m_txtCharacterFilter_TextChanged(object sender, EventArgs e)
		{
			LoadCharacterListBox(m_viewModel.GetUniqueCharacters(m_txtCharacterFilter.Text));
		}

		private void m_listBoxCharacters_SelectedIndexChanged(object sender, EventArgs e)
		{
			var selectedCharacter = (AssignCharacterViewModel.Character)m_listBoxCharacters.SelectedItem;

			LoadDeliveryListBox(m_viewModel.GetDeliveriesForCharacter(selectedCharacter));
			HideDeliveryFilter();
			if (selectedCharacter != null && selectedCharacter.IsNarrator)
				m_llMoreDel.Enabled = false;
			UpdateOkButtonState();
		}

		private void m_listBoxDeliveries_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateOkButtonState();
		}

		private void m_listBoxCharacters_KeyPress(object sender, KeyPressEventArgs e)
		{
			HandleCharacterSelectionKeyPress(e);
			e.Handled = true;
		}

		private void HandleCharacterSelectionKeyPress(KeyPressEventArgs e)
		{
			if (Char.IsLetter(e.KeyChar))
			{
				var charactersStartingWithSelectedLetter =
					CurrentContextCharacters.Where(c => c.ToString().StartsWith(e.KeyChar.ToString(CultureInfo.InvariantCulture), true, CultureInfo.InvariantCulture));
				if (charactersStartingWithSelectedLetter.Count() == 1)
					m_listBoxCharacters.SelectedItem = charactersStartingWithSelectedLetter.Single();
				else
					m_listBoxCharacters.SelectedItem = null;
			}
		}

		private void m_listBoxCharacters_MouseMove(object sender, MouseEventArgs e)
		{
			int newHoveredIndex = m_listBoxCharacters.IndexFromPoint(e.Location);

			if (m_characterListHoveredIndex != newHoveredIndex)
			{
				m_characterListHoveredIndex = newHoveredIndex;
				if (m_characterListHoveredIndex > -1)
				{
					m_characterListToolTip.Active = false;
					var hoveredCharacter = ((AssignCharacterViewModel.Character)m_listBoxCharacters.Items[m_characterListHoveredIndex]);
					if (!IsNullOrEmpty(hoveredCharacter.LocalizedAlias))
					{
						m_characterListToolTip.SetToolTip(m_listBoxCharacters, hoveredCharacter.LocalizedCharacterId);
						m_characterListToolTip.Active = true;
					}
				}
			}
		}

		private void m_llMoreDel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			ShowDeliveryFilter();
			LoadDeliveryListBox(m_viewModel.GetUniqueDeliveries(), (AssignCharacterViewModel.Delivery)m_listBoxDeliveries.SelectedItem);
			m_txtDeliveryFilter.Focus();
			var fc = this.FindFocusedControl();
			if (fc != null)
				Debug.WriteLine("m_llMoreDel_LinkClicked - Focus: " + fc.Name);
		}

		private void ShowCharactersInBook()
		{
			LoadCharacterListBox(m_viewModel.GetUniqueCharacters());
		}

		private void LoadCharacterListBox(IEnumerable<AssignCharacterViewModel.Character> characters)
		{
			m_listBoxCharacters.BeginUpdate();

			m_listBoxCharacters.Items.Clear();
			m_listBoxDeliveries.Items.Clear();
			HideDeliveryFilter();

			foreach (var character in characters)
				m_listBoxCharacters.Items.Add(character);
			SelectCharacter();

			m_listBoxCharacters.EndUpdate();
			UpdateOkButtonState();
		}

		private void SelectCharacter()
		{
			var character = m_viewModel.GetCharacterToSelectForCurrentBlock(CurrentContextCharacters);
			if (character != null)
				m_listBoxCharacters.SelectedItem = character;
		}

		private void ShowDeliveryFilter()
		{
			m_pnlDeliveryFilter.Show();
			m_btnAddDelivery.Show();
			m_llMoreDel.Enabled = false;
		}

		private void HideDeliveryFilter()
		{
			m_txtDeliveryFilter.Clear();
			m_pnlDeliveryFilter.Hide();
			m_btnAddDelivery.Hide();
			m_llMoreDel.Enabled = true;
		}

		private void LoadDeliveryListBox(IEnumerable<AssignCharacterViewModel.Delivery> deliveries, AssignCharacterViewModel.Delivery selectedItem = null)
		{
			m_listBoxDeliveries.BeginUpdate();
			m_listBoxDeliveries.Items.Clear();

			foreach (var delivery in deliveries)
				m_listBoxDeliveries.Items.Add(delivery);

			SelectDelivery(selectedItem);
			m_listBoxDeliveries.EndUpdate();
		}

		private void SelectDelivery(AssignCharacterViewModel.Delivery previouslySelectedDelivery)
		{
			if (m_listBoxCharacters.Items.Count == 0 || m_listBoxDeliveries.Items.Count == 0 || m_listBoxCharacters.SelectedItem == null)
				return;
			Block currentBlock = m_viewModel.CurrentBlock;
			string currentDelivery = IsNullOrEmpty(currentBlock.Delivery) ? AssignCharacterViewModel.Delivery.Normal.Text : currentBlock.Delivery;

			if (m_listBoxDeliveries.Items.Count == 1)
				m_listBoxDeliveries.SelectedIndex = 0;
			else
			{
				if (currentBlock.CharacterId == ((AssignCharacterViewModel.Character)m_listBoxCharacters.SelectedItem).CharacterId)
				{
					foreach (var delivery in m_listBoxDeliveries.Items.Cast<AssignCharacterViewModel.Delivery>())
					{
						if (delivery.Text == currentDelivery)
						{
							m_listBoxDeliveries.SelectedItem = delivery;
							return;
						}
					}
				}
				else if (m_listBoxDeliveries.Items.Count == 2)
					m_listBoxDeliveries.SelectedIndex = 1; // The first one will always be "Normal", so choose the other one.
			}

			if (m_listBoxDeliveries.SelectedItem == null && previouslySelectedDelivery != null)
			{
				if (m_listBoxDeliveries.Items.Cast<AssignCharacterViewModel.Delivery>().Any(delivery => delivery == previouslySelectedDelivery))
				{
					m_listBoxDeliveries.SelectedItem = previouslySelectedDelivery;
				}
			}
		}

		private void UpdateOkButtonState()
		{
			m_btnOk.Enabled = IsCharacterAndDeliverySelectionComplete && IsDirty;
			if (m_btnOk.Enabled && !m_btnOk.Focused)
			{
				var focusedControl = this.FindFocusedControl();
				if (focusedControl is Button || focusedControl is LinkLabel)
					m_btnOk.Focus();
			}
		}
	}
}
