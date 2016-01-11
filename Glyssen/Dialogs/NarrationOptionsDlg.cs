using System;
using Glyssen.Utilities;

namespace Glyssen.Dialogs
{
	public partial class NarrationOptionsDlg : CustomForm
	{
		private readonly Project m_project;

		public NarrationOptionsDlg(Project project)
		{
			InitializeComponent();
			m_project = project;

			m_project.SetDefaultCharacterGroupGenerationPreferences();
			m_numMaleNarrator.Value = m_project.CharacterGroupGenerationPreferences.NumberOfMaleNarrators;
			m_numMaleNarrator.Maximum = m_project.IncludedBooks.Count;
			m_numFemaleNarrator.Value = m_project.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators;
			m_numFemaleNarrator.Maximum = m_project.IncludedBooks.Count;
		}

		private void BtnOk_Clicked(object sender, EventArgs e)
		{
			var pref = m_project.CharacterGroupGenerationPreferences;
			pref.NumberOfMaleNarrators = (int)m_numMaleNarrator.Value;
			pref.NumberOfFemaleNarrators = (int)m_numFemaleNarrator.Value;

			if (pref.NumberOfMaleNarrators == 0 && pref.NumberOfFemaleNarrators == 0)
				pref.NumberOfMaleNarrators = 1; // REVIEW: Should this rest to default instead?

			pref.IsSetByUser = true;

			m_project.Save();
		}
	}
}
