using System;
using System.Windows.Forms;

namespace GlyssenApp.UI.Dialogs
{
	public partial class PercentageOfExpectedQuotesFoundTooLowDlg : Form
	{
		public PercentageOfExpectedQuotesFoundTooLowDlg(string caption, double percentageOfExpectedQuotesFound)
		{
			const double minimumTarget = 80;

			InitializeComponent();

			Text = caption;

			m_lblOnlyNPercentOfExpectedQuotesFound.Text = String.Format(m_lblOnlyNPercentOfExpectedQuotesFound.Text, percentageOfExpectedQuotesFound);

			if (percentageOfExpectedQuotesFound < minimumTarget)
			{
				m_lblPossibleProblemsWithFirstLevelQuotes.Text = String.Format(m_lblPossibleProblemsWithFirstLevelQuotes.Text, minimumTarget);
				m_lblPossibleProblemsWithLowerLevelQuotesOrDialogueDash.Visible = false;
				m_lblDirectSpeechNotMarked.Visible = false;
			}
			else
			{
				m_lblPossibleProblemsWithLowerLevelQuotesOrDialogueDash.Text = String.Format(m_lblPossibleProblemsWithLowerLevelQuotesOrDialogueDash.Text,
					Glyssen.Properties.Settings.Default.TargetPercentageOfQuotesFound);
				m_lblPossibleProblemsWithFirstLevelQuotes.Visible = false;
			}
		}

		public bool UserWantsToReview
		{
			get { return m_rdoReview.Checked; }
		}
	}
}
