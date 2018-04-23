using System;

namespace Waxuquerque
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
					throw new InvalidOperationException("Not Initialized. Set Logger.Default first.");
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

		#region unfortunately copied from SIL.Desktop
		//TODO refactoring figure out how to clean this up
		private static bool s_justRecordNonFatalMessagesForTesting;
		private static string s_previousNonFatalMessage;
		private static Exception s_previousNonFatalException;

		/// <summary>
		/// use this in unit tests to cleanly check that a message would have been shown.
		/// E.g.  using (new Palaso.Reporting.ErrorReport.NonFatalErrorReportExpected()) {...}
		/// </summary>
		public class NonFatalErrorReportExpected : IDisposable
		{
			private readonly bool m_previousJustRecordNonFatalMessagesForTesting;
			public NonFatalErrorReportExpected()
			{
				m_previousJustRecordNonFatalMessagesForTesting = s_justRecordNonFatalMessagesForTesting;
				s_justRecordNonFatalMessagesForTesting = true;
				s_previousNonFatalMessage = null;//this is a static, so a previous unit test could have filled it with something (yuck)
			}
			public void Dispose()
			{
				s_justRecordNonFatalMessagesForTesting = m_previousJustRecordNonFatalMessagesForTesting;
				if (s_previousNonFatalException == null && s_previousNonFatalMessage == null)
					throw new Exception("Non Fatal Error Report was expected but wasn't generated.");
				s_previousNonFatalMessage = null;
			}
			/// <summary>
			/// use this to check the actual contents of the message that was triggered
			/// </summary>
			public string Message => s_previousNonFatalMessage;
		}

		/// <summary>
		/// use this in unit tests to cleanly check that a message would have been shown.
		/// E.g.  using (new Palaso.Reporting.ErrorReport.NonFatalErrorReportExpected()) {...}
		/// </summary>
		public class NoNonFatalErrorReportExpected : IDisposable
		{
			private readonly bool m_previousJustRecordNonFatalMessagesForTesting;
			public NoNonFatalErrorReportExpected()
			{
				m_previousJustRecordNonFatalMessagesForTesting = s_justRecordNonFatalMessagesForTesting;
				s_justRecordNonFatalMessagesForTesting = true;
				s_previousNonFatalMessage = null;//this is a static, so a previous unit test could have filled it with something (yuck)
				s_previousNonFatalException = null;
			}
			public void Dispose()
			{
				s_justRecordNonFatalMessagesForTesting = m_previousJustRecordNonFatalMessagesForTesting;
				if (s_previousNonFatalException != null || s_previousNonFatalMessage != null)
					throw new Exception("Non Fatal Error Report was not expected but was generated: " + Message);
				s_previousNonFatalMessage = null;
			}
			/// <summary>
			/// use this to check the actual contents of the message that was triggered
			/// </summary>
			public string Message => s_previousNonFatalMessage;
		}
		#endregion
	}
}