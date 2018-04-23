using System;

namespace Glyssen
{
	public interface ILogger
	{
		void WriteEvent(string message, params object[] args);

		void WriteMinorEvent(string message, params object[] args);

		void WriteError(Exception e);

		void WriteError(string msg, Exception e);
	}

	public static class Logger
	{
		private static ILogger s_instance;

		public static ILogger Default
		{
			get
			{
				if (s_instance == null)
					throw new InvalidOperationException("Not Initialized. Set Logger.Default first.");
				return s_instance;
			}
			set => s_instance = value;
		}

		internal static void WriteEvent(string message, params object[] args)
		{
			Default.WriteEvent(message, args);
		}

		internal static void WriteMinorEvent(string message, params object[] args)
		{
			Default.WriteMinorEvent(message, args);
		}

		internal static void WriteError(Exception e)
		{
			Default.WriteError(e);
		}

		internal static void WriteError(string msg, Exception e)
		{
			Default.WriteError(msg, e);
		}
	}
}