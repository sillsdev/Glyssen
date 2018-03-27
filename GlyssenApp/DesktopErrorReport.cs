using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SIL.Reporting;

namespace GlyssenApp
{
	class DesktopErrorReport : Glyssen.IErrorReport
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
