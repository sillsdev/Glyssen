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

		public static void SetInstance(ILogger instance)
		{
			s_instance = instance;
		}

		internal static void WriteEvent(string message, params object[] args)
		{
			if (s_instance == null)
				throw new InvalidOperationException("You must call SetInstance() first.");

			s_instance.WriteEvent(message, args);
		}

		internal static void WriteMinorEvent(string message, params object[] args)
		{
			if (s_instance == null)
				throw new InvalidOperationException("You must call SetInstance() first.");

			s_instance.WriteMinorEvent(message, args);
		}

		internal static void WriteError(Exception e)
		{
			if (s_instance == null)
				throw new InvalidOperationException("You must call SetInstance() first.");

			s_instance.WriteError(e);
		}

		internal static void WriteError(string msg, Exception e)
		{
			if (s_instance == null)
				throw new InvalidOperationException("You must call SetInstance() first.");

			s_instance.WriteError(msg, e);
		}
	}
}