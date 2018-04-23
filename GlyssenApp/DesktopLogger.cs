using System;
using SIL.Reporting;
using ILogger = Waxuquerque.ILogger;

namespace GlyssenApp
{
	class DesktopLogger : ILogger
	{
		public void WriteEvent(string message, params object[] args)
		{
			Logger.WriteEvent(message, args);
		}

		public void WriteMinorEvent(string message, params object[] args)
		{
			Logger.WriteMinorEvent(message, args);
		}

		public void WriteError(Exception e)
		{
			Logger.WriteError(e);
		}

		public void WriteError(string msg, Exception e)
		{
			Logger.WriteError(msg, e);
		}
	}
}
