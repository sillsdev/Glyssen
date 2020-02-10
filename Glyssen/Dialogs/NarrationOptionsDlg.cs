using System;
using System.Windows.Forms;
using GlyssenEngine;
using L10NSharp;

namespace Glyssen.Dialogs
{
	public partial class NarrationOptionsDlg : Form
	{
		private readonly Project m_project;
		private readonly int m_includedBooksCount;

		public NarrationOptionsDlg(Project project)
		{
			InitializeComponent();
			m_project = project;
			m_includedBooksCount = m_project.IncludedBooks.Count;

			m_project.SetDefaultCharacterGroupGenerationPreferences();
			m_numMaleNarrator.Value = m_project.CharacterGroupGenerationPreferences.NumberOfMaleNarrators;
			m_numMaleNarrator.Maximum = m_includedBooksCount;
			m_numFemaleNarrator.Value = m_project.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators;
			m_numFemaleNarrator.Maximum = m_includedBooksCount;
		}

		private void BtnOk_Clicked(object sender, EventArgs e)
		{
			int numMaleNarrators = (int)m_numMaleNarrator.Value;
			int numFemaleNarrators = (int)m_numFemaleNarrator.Value;

			if ((numMaleNarrators == 0 && numFemaleNarrators == 0) || (numMaleNarrators + numFemaleNarrators > m_includedBooksCount))
			{
				string caption = LocalizationManager.GetString("DialogBoxes.NarrationOptionsDlg.InvalidNumNarrators.Caption",
					"Invalid Number Of Narrators Requested");
				string msg = string.Format(LocalizationManager.GetString("DialogBoxes.NarrationOptionsDlg.InvalidNumNarrators.MessageText",
					"Total number of narrators must be more than zero and less than or equal to the number of books ({0} for this script)."), m_includedBooksCount);
				MessageBox.Show(msg, caption);
				DialogResult = DialogResult.None;
				return;
			}

			var pref = m_project.CharacterGroupGenerationPreferences;
			pref.NumberOfMaleNarrators = numMaleNarrators;
			pref.NumberOfFemaleNarrators = numFemaleNarrators;
			pref.IsSetByUser = true;

			m_project.Save();
		}
	}
}
