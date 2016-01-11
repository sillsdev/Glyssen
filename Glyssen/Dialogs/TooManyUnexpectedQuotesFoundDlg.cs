using System;
using Glyssen.Utilities;

namespace Glyssen.Dialogs
{
	public partial class TooManyUnexpectedQuotesFoundDlg : CustomForm
	{
		public TooManyUnexpectedQuotesFoundDlg(string caption, double percentageOfQuotesFoundThatWereUnexpected)
		{
			InitializeComponent();

			Text = caption;

			m_lblOnlyNPercentOfExpectedQuotesFound.Text = String.Format(m_lblOnlyNPercentOfExpectedQuotesFound.Text, percentageOfQuotesFoundThatWereUnexpected);

			m_lblPossibleProblemsWithFirstLevelQuotes.Text = String.Format(m_lblPossibleProblemsWithFirstLevelQuotes.Text, Properties.Settings.Default.MaxAcceptablePercentageOfUnknownQuotes);
		}

		public bool UserWantsToReview
		{
			get { return m_rdoReview.Checked; }
		}
	}
}
