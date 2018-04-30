using System;

namespace Waxuquerque.Utilities
{
	public interface IErrorReport
	{
		void ReportNonFatalMessageWithStackTrace(string message, params object[] args);

		void ReportNonFatalExceptionWithMessage(Exception error, string message, params object[] args);

		void NotifyUserOfProblem(string message, params object[] args);

		void NotifyUserOfProblem(Exception error, string messageFmt, params object[] args);
	}

	public static class ErrorReport
	{
		private static IErrorReport s_instance;

		public static IErrorReport Default
		{
			get
			{
				if (s_instance == null)
					throw new InvalidOperationException("Not Initialized. Set ErrorReport.Default first.");
				return s_instance;
			}
			set => s_instance = value;
		}

		internal static void ReportNonFatalMessageWithStackTrace(string message, params object[] args)
		{
			Default.ReportNonFatalMessageWithStackTrace(message, args);
		}

		internal static void ReportNonFatalExceptionWithMessage(Exception error, string message, params object[] args)
		{
			Default.ReportNonFatalExceptionWithMessage(error, message, args);
		}

		internal static void NotifyUserOfProblem(string message, params object[] args)
		{
			Default.NotifyUserOfProblem(message, args);
		}

		internal static void NotifyUserOfProblem(Exception error, string messageFmt, params object[] args)
		{
			Default.NotifyUserOfProblem(error, messageFmt, args);
		}
	}
}