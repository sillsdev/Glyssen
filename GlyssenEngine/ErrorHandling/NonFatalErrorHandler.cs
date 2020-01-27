using System;
using System.Collections.Generic;
using SIL.Reporting;

namespace GlyssenEngine.ErrorHandling
{
    public interface IErrorHandler
    {
        void HandleException(Exception e, string context = null, Dictionary<string, string> details = null);
    }

	public enum ErrorHandlingOptions
	{
		Default,
		Log,
		Report,
	}

    public abstract class NonFatalErrorHandler
    {
        public static IErrorHandler Default { get; set; }

        public static void HandleException(Exception e, ErrorHandlingOptions options = ErrorHandlingOptions.Default, string message = null, Dictionary<string, string> details = null)
        {
			if (options == ErrorHandlingOptions.Log)
				Logger.WriteError(e);
			else if (options == ErrorHandlingOptions.Report)
			{
				ErrorReport.NotifyUserOfProblem(e, message);

				// If the message was reported to the user, we assume it contains details that might not be appropriate to
				// pass along to the "outside" world (e.g., Analytics)
				Default?.HandleException(e);
				return;
			}
			Default?.HandleException(e, message, details);
        }
	}
}