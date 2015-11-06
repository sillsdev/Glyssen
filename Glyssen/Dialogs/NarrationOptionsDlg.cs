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
		private const int kInvalidNarratorNum = -1;
		private int m_intNarratorNum = kInvalidNarratorNum;
		private readonly Project m_project;

		public NarrationOptionsDlg(Project project)
		{
			InitializeComponent();
			m_project = project;

			if (m_project.NumberOfNarrators != GlyssenDblTextMetadata.kNumberOfNarratorsNotSet)
			{
				m_intNarratorNum = m_project.NumberOfNarrators;
				m_txtNarratorNum.Text = m_intNarratorNum.ToString();
			}

			switch (m_project.NarratorGenders)
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

		private bool m_validateAndSetNarratorNum()
		{
			if (int.TryParse(m_txtNarratorNum.Text, out m_intNarratorNum))
				if (m_intNarratorNum > 0)
					return true;
			m_intNarratorNum = kInvalidNarratorNum;
			return false;
		}

		private void m_txtNarratorNum_TextChanged(object sender, EventArgs e)
		{
			if (m_validateAndSetNarratorNum())
			{
				m_btnOk.Enabled = true;
				m_txtNarratorNum.BackColor = Color.White;
			}
			else
			{
				m_btnOk.Enabled = false;
				m_txtNarratorNum.BackColor = Color.Yellow;
			}
		}

		private void m_btnOk_Clicked(object sender, EventArgs e)
		{
			m_project.NumberOfNarrators = m_intNarratorNum;
			if(m_rdoFemaleOnly.Checked)
				m_project.NarratorGenders = NarratorGenders.FemaleOnly;
			else if(m_rdoMaleOrFemale.Checked)
				m_project.NarratorGenders = NarratorGenders.MaleOrFemale;
			else
				m_project.NarratorGenders = NarratorGenders.MaleOnly;

			m_project.Save();

			DialogResult = DialogResult.OK;
			Close();
		}

		private void m_btnCancel_Clicked(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}
	}
}
