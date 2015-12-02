using System;
using System.Windows.Forms;
using Glyssen.Bundle;

namespace Glyssen.Dialogs
{
	public partial class NarrationOptionsDlg : Form
	{
		private readonly Project m_project;

		public NarrationOptionsDlg(Project project)
		{
			InitializeComponent();
			m_project = project;

			if (m_project.CharacterGroupGenerationPreferences.NumberOfNarrators != CharacterGroupGenerationPreferences.kNumberOfNarratorsNotSet)
				m_numNarratorNum.Value = m_project.CharacterGroupGenerationPreferences.NumberOfNarrators;
			else
				m_numNarratorNum.Value = GetDefaultNumberOfNarrators();
			m_numNarratorNum.Maximum = m_project.IncludedBooks.Count;

			switch (m_project.CharacterGroupGenerationPreferences.NarratorGenders)
			{
				case NarratorGenders.FemaleOnly:
					m_rdoFemaleOnly.Checked = true;
					break;
				case NarratorGenders.MaleOrFemale:
					m_rdoMaleOrFemale.Checked = true;
					break;
				default:
					m_rdoMaleOnly.Checked = true;
					break;

			}
		}

		private int GetDefaultNumberOfNarrators()
		{
			//right now - fairly arbitrary
			return Math.Min(5, m_project.IncludedBooks.Count);
		}

		private void BtnOk_Clicked(object sender, EventArgs e)
		{
			m_project.CharacterGroupGenerationPreferences.NumberOfNarrators = (int)m_numNarratorNum.Value;
			if (m_rdoFemaleOnly.Checked)
				m_project.CharacterGroupGenerationPreferences.NarratorGenders = NarratorGenders.FemaleOnly;
			else if (m_rdoMaleOrFemale.Checked)
				m_project.CharacterGroupGenerationPreferences.NarratorGenders = NarratorGenders.MaleOrFemale;
			else
				m_project.CharacterGroupGenerationPreferences.NarratorGenders = NarratorGenders.MaleOnly;

			m_project.Save();
		}
	}
}
