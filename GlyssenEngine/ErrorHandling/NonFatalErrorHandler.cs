using System;
using System.Collections.Generic;
using SIL.Reporting;

namespace GlyssenEngine.ErrorHandling
{
    public interface IErrorHandler
    {
        void HandleException(Exception e, string context = null, Dictionary<string, string> details = null);
    }

	public static class NonFatalErrorHandler
    {
        public static IErrorHandler Default { get; set; }

        public static void HandleException(Exception e, string message = null, Dictionary<string, string> details = null)
        {
	        Default?.HandleException(e, message, details);
		}

		public static void LogAndHandleException(Exception e, string message, Dictionary<string, string> details = null)
		{
			Logger.WriteError(e);
			HandleException(e, message, details);
		}

		/// <summary>
		/// When reporting an exception to the user, we assume that they will normally (when appropriate) communicate
		/// the problem back to the development team, so we will have all the details to be able to follow up as needed.
		/// We intentionally omit the <paramref name="message"/> when passing the exception along to the default
		/// exception handler because it likely contains details that might not be appropriate to pass along to the
		/// "outside" world (e.g., Analytics).
		/// </summary>
		/// <param name="e">The exception</param>
		/// <param name="message">Additional explanatory information to report</param>
		public static void ReportAndHandleException(Exception e, string message)
        {
			ErrorReport.NotifyUserOfProblem(e, message);
			HandleException(e);
        }
	}
}