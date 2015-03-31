using System;
using System.Windows.Forms;
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
			foreach (var book in m_project.AvailableBooks)
				book.IncludeInScript = m_bookChooserControl.SelectedBooks.IsSelected(BCVRef.BookToNumber(book.Code));
			m_project.IsBookSelectionUserConfirmed = true;
		}
	}
}
