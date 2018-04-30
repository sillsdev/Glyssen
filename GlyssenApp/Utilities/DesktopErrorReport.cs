using System;
using Waxuquerque;
using Waxuquerque.Utilities;
using ErrorReport = SIL.Reporting.ErrorReport;

namespace GlyssenApp.Utilities
{
	class DesktopErrorReport : IErrorReport
	{
		public void ReportNonFatalMessageWithStackTrace(string message, params object[] args)
		{
			ErrorReport.ReportNonFatalMessageWithStackTrace(message, args);
		}

		public void ReportNonFatalExceptionWithMessage(Exception error, string message, params object[] args)
		{
			ErrorReport.ReportNonFatalExceptionWithMessage(error, message, args);
		}

		public void NotifyUserOfProblem(string message, params object[] args)
		{
			ErrorReport.NotifyUserOfProblem(message, args);
		}

		public void NotifyUserOfProblem(Exception error, string messageFmt, params object[] args)
		{
			ErrorReport.NotifyUserOfProblem(error, messageFmt, args);
		}
	}
}
