using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Glyssen.Bundle;

namespace Glyssen
{
	public partial class NarrationOptionsDlg : Form
	{
		private readonly Project m_project;

		public NarrationOptionsDlg(Project project)
		{
			InitializeComponent();
			m_project = project;

			if (m_project.ProjectSettings.NumberOfNarrators != ProjectSettings.kNumberOfNarratorsNotSet)
				m_numNarratorNum.Value = m_project.ProjectSettings.NumberOfNarrators;
			else
				m_numNarratorNum.Value = m_GoodDefaultNarratorNum();
			m_numNarratorNum.Maximum = m_project.IncludedBooks.Count;

			switch (m_project.ProjectSettings.NarratorGenders)
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

		private int m_GoodDefaultNarratorNum()
		{
			//right now - fairly arbitrary
			return Math.Min(5, m_project.IncludedBooks.Count);
		}

		private void m_btnOk_Clicked(object sender, EventArgs e)
		{
			m_project.ProjectSettings.NumberOfNarrators = (int)m_numNarratorNum.Value;
			if(m_rdoFemaleOnly.Checked)
				m_project.ProjectSettings.NarratorGenders = NarratorGenders.FemaleOnly;
			else if(m_rdoMaleOrFemale.Checked)
				m_project.ProjectSettings.NarratorGenders = NarratorGenders.MaleOrFemale;
			else
				m_project.ProjectSettings.NarratorGenders = NarratorGenders.MaleOnly;

			m_project.Save();

			DialogResult = DialogResult.OK;
			Close();
		}
	}
}
