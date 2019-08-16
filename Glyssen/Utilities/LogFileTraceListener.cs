using System;
using System.Diagnostics;
using System.Threading;
using SIL.Reporting;

namespace Glyssen.Utilities
{
	class LogFileTraceListener : TraceListener
	{
		public override void Write(string message)
		{
			WriteLine(message);
		}

		public override void WriteLine(string message)
		{
			Logger.WriteEvent(message);
		}

		public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id,
			string format, params object[] args)
		{
			// Write timestamp and calling method
			WriteLine($"[{DateTime.Now:yyyy-MM-dd'T'HH:mm:sszz}, Thread={ThreadId}] - {eventType}: ");
			WriteLine(string.Format(format, args));
		}

		/// <summary>
		/// Calculate a unique thread description.
		/// </summary>
		/// <returns>unique thread description</returns>
		private static string ThreadId =>
			Thread.CurrentThread.Name == null ? "Unnamed thread" : Thread.CurrentThread.Name +
				$"({Thread.CurrentThread.ManagedThreadId})";
	}
}
