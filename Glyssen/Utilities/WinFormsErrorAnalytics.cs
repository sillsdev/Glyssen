using System;
using System.Collections.Generic;
using DesktopAnalytics;
using GlyssenEngine.ErrorHandling;

namespace Glyssen.Utilities
{
	class WinFormsErrorAnalytics : IErrorHandler
	{
		public const string kExceptionMsgKey = "exceptionMessage";
		public void HandleException(Exception e, string context = null, Dictionary<string, string> details = null)
		{
			if (context == null)
				Analytics.ReportException(e);
			else
			{
				if (details == null)
					details = new Dictionary<string, string> { { kExceptionMsgKey, e.Message } };
				else
					details.Add(kExceptionMsgKey, e.Message);
				Analytics.Track(context, details);
			}
		}
	}
}
