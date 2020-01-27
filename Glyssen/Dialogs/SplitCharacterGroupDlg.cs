using System.Collections;
using System.Linq;
using System.Windows.Forms;
using Glyssen.Character;
using GlyssenEngine.Character;
using GlyssenEngine.ViewModels;

namespace Glyssen.Dialogs
{
	public partial class SplitCharacterGroupDlg : Form
	{
		private readonly CharacterGroup m_existingGroup;
		private readonly VoiceActorAssignmentViewModel m_model;

		public SplitCharacterGroupDlg(CharacterGroup existingGroup, VoiceActorAssignmentViewModel model)
		{
			m_existingGroup = existingGroup;
			m_model = model;

			InitializeComponent();

			PopulateListBox();
		}

		public CharacterGroup NewGroup { get; private set; }

		private void PopulateListBox()
		{
			m_listboxExisting.Items.AddRange(m_existingGroup.CharacterIds.Select(c => new CharacterIds(c)).OrderBy(c => c.LocalizedCharacterId).ToArray());
		}

		private void MoveSelectedItems(ListBox from, ListBox to)
		{
			foreach (var item in from.SelectedItems)
				to.Items.Add(item);

			foreach (var item in new ArrayList(from.SelectedItems))
				from.Items.Remove(item);

			m_btnOk.Enabled = from.Items.Count > 0 && to.Items.Count > 0;
		}

		private void m_btnAdd_Click(object sender, System.EventArgs e)
		{
			MoveSelectedItems(m_listboxExisting, m_listboxNew);
		}

		private void m_btnRemove_Click(object sender, System.EventArgs e)
		{
			MoveSelectedItems(m_listboxNew, m_listboxExisting);
		}

		private void Listboxes_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			m_btnAdd.Enabled = m_listboxExisting.SelectedItems.Count > 0;
			m_btnRemove.Enabled = m_listboxNew.SelectedItems.Count > 0;
		}

		private void m_btnOk_Click(object sender, System.EventArgs e)
		{
			NewGroup = m_model.SplitGroup(m_listboxNew.Items.Cast<CharacterIds>().Select(c => c.CharacterId).ToList());
		}

		private class CharacterIds
		{
			public CharacterIds(string characterId)
			{
				CharacterId = characterId;
				LocalizedCharacterId = CharacterVerseData.GetCharacterNameForUi(characterId);
			}

			public string CharacterId { get; private set; }
			public string LocalizedCharacterId { get; private set; }

			public override string ToString()
			{
				return LocalizedCharacterId;
			}
		}
	}
}
