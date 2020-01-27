using System.Data;
using System.Windows.Forms;
using GlyssenEngine.Character;
using L10NSharp;
using L10NSharp.TMXUtils;
using L10NSharp.UI;

namespace Glyssen.Dialogs
{
	public partial class NewCharacterDlg : Form
	{
		private readonly string m_characterId;

		public NewCharacterDlg(string characterId)
		{
			m_characterId = characterId;

			InitializeComponent();

			HandleStringsLocalized();
			LocalizeItemDlg<TMXDocument>.StringsLocalized += HandleStringsLocalized;

			PopulateComboBoxes();
		}

		public CharacterGender Gender
		{
			get
			{
				var dataTable = (DataTable)m_cmbGender.DataSource;
				return (CharacterGender)dataTable.Rows[m_cmbGender.SelectedIndex][m_cmbGender.ValueMember];
			}
		}

		public CharacterAge Age
		{
			get
			{
				var dataTable = (DataTable)m_cmbAge.DataSource;
				return (CharacterAge)dataTable.Rows[m_cmbAge.SelectedIndex][m_cmbAge.ValueMember];
			}
		}

		private void HandleStringsLocalized()
		{
			m_lblInstructions.Text = string.Format(m_lblInstructions.Text, m_characterId);
		}

		private DataTable GetGenderDataTable()
		{
			var table = new DataTable();
			table.Columns.Add("ID", typeof(CharacterGender));
			table.Columns.Add("Name");
			table.Rows.Add(CharacterGender.Male, GetUiStringForCharacterGender(CharacterGender.Male));
			table.Rows.Add(CharacterGender.Female, GetUiStringForCharacterGender(CharacterGender.Female));
			table.Rows.Add(CharacterGender.Either, GetUiStringForCharacterGender(CharacterGender.Either));
			table.Rows.Add(CharacterGender.PreferMale, GetUiStringForCharacterGender(CharacterGender.PreferMale));
			table.Rows.Add(CharacterGender.PreferFemale, GetUiStringForCharacterGender(CharacterGender.PreferFemale));
			table.Rows.Add(CharacterGender.Neuter, GetUiStringForCharacterGender(CharacterGender.Neuter));
			return table;
		}

		private static string GetUiStringForCharacterGender(CharacterGender characterGender)
		{
			switch (characterGender)
			{
				case CharacterGender.Male: return LocalizationManager.GetString("DialogBoxes.NewCharacterDlg.CharacterGender.Male", "Male");
				case CharacterGender.Female: return LocalizationManager.GetString("DialogBoxes.NewCharacterDlg.CharacterGender.Female", "Female");
				case CharacterGender.PreferMale: return LocalizationManager.GetString("DialogBoxes.NewCharacterDlg.CharacterGender.PreferMale", "Either (Male Preferred)");
				case CharacterGender.PreferFemale: return LocalizationManager.GetString("DialogBoxes.NewCharacterDlg.CharacterGender.PreferFemale", "Either (Female Preferred)");
				case CharacterGender.Neuter: return LocalizationManager.GetString("DialogBoxes.NewCharacterDlg.CharacterGender.Neuter", "Neuter");
				default: return LocalizationManager.GetString("DialogBoxes.NewCharacterDlg.CharacterGender.Either", "Either");
			}
		}

		private DataTable GetAgeDataTable()
		{
			var table = new DataTable();
			table.Columns.Add("ID", typeof(CharacterAge));
			table.Columns.Add("Name");
			table.Rows.Add(CharacterAge.Adult, GetUiStringForCharacterAge(CharacterAge.Adult));
			table.Rows.Add(CharacterAge.Elder, GetUiStringForCharacterAge(CharacterAge.Elder));
			table.Rows.Add(CharacterAge.YoungAdult, GetUiStringForCharacterAge(CharacterAge.YoungAdult));
			table.Rows.Add(CharacterAge.Child, GetUiStringForCharacterAge(CharacterAge.Child));
			return table;
		}

		private static string GetUiStringForCharacterAge(CharacterAge characterAge)
		{
			switch (characterAge)
			{
				case CharacterAge.Child: return LocalizationManager.GetString("DialogBoxes.NewCharacterDlg.CharacterAge.Child", "Child");
				case CharacterAge.Elder: return LocalizationManager.GetString("DialogBoxes.NewCharacterDlg.CharacterAge.Elder", "Elder");
				case CharacterAge.YoungAdult: return LocalizationManager.GetString("DialogBoxes.NewCharacterDlg.CharacterAge.YoungAdult", "Young Adult");
				default: return LocalizationManager.GetString("DialogBoxes.NewCharacterDlg.CharacterAge.Adult", "Adult");
			}
		}

		private void PopulateComboBoxes()
		{
			m_cmbGender.DataSource = GetGenderDataTable();
			m_cmbGender.ValueMember = "ID";
			m_cmbGender.DisplayMember = "Name";

			m_cmbAge.DataSource = GetAgeDataTable();
			m_cmbAge.ValueMember = "ID";
			m_cmbAge.DisplayMember = "Name";
		}
	}
}
