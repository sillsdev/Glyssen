using System;
using System.Linq;
using System.Windows.Forms;
using ProtoScript.Bundle;
using SIL.ScriptureUtils;

namespace ProtoScript.Dialogs
{
	public partial class ScriptureRangeSelectionDialog : Form
	{
		private readonly Project m_project;

		public ScriptureRangeSelectionDialog()
		{
			InitializeComponent();
		}

		public ScriptureRangeSelectionDialog(Project project) : this()
		{
			m_project = project;
			m_bookChooserControl.Setup(project.AvailableBooks.ToBookSet(), project.IncludedBooks.ToBookSet());
			m_bookChooserControl.BooksChanged += UpdateState;
		}

		private void UpdateState(object sender, EventArgs e)
		{
			m_btnOk.Enabled = m_bookChooserControl.SelectedBooks.Count > 0;
		}

		private void m_btnOk_Click(object sender, EventArgs e)
		{
			foreach (string bookCode in m_bookChooserControl.SelectedBooks.SelectedBookNumbers.Select(BCVRef.NumberToBookCode))
			{
				Book metadataBook = m_project.AvailableBooks.FirstOrDefault(b => b.Code == bookCode);
				if (metadataBook != null)
					metadataBook.IncludeInScript = true;
			}
			foreach (string bookCode in m_bookChooserControl.SelectedBooks.UnselectedBookNumbers.Select(BCVRef.NumberToBookCode))
			{
				Book metadataBook = m_project.AvailableBooks.FirstOrDefault(b => b.Code == bookCode);
				if (metadataBook != null)
					metadataBook.IncludeInScript = false;
			}
		}
	}
}
