using System;
using System.Collections.Generic;
using System.Windows.Forms;
using DesktopAnalytics;
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
			foreach (var book in m_project.AvailableBooks)
				book.IncludeInScript = m_bookChooserControl.SelectedBooks.IsSelected(BCVRef.BookToNumber(book.Code));
			m_project.BookSelectionStatus = BookSelectionStatus.Reviewed;

			Analytics.Track("SelectBooks", new Dictionary<string, string> { { "bookSummary", m_project.BookSelectionSummary } });
		}
	}
}
