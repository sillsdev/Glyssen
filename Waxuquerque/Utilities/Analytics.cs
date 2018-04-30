using System;
using System.Collections.Generic;

namespace Waxuquerque.Utilities
{
	public interface IAnalytics
	{
		void Track(string eventName, Dictionary<string, string> properties);
		void ReportException(Exception e);
	}

	public abstract class Analytics
	{
		private static IAnalytics s_analytics = new NullAnalytics();

		public static IAnalytics Default
		{
			get
			{
				if (s_analytics == null)
					throw new InvalidOperationException("Not initialized. Set Analytics.Default first.");

				return s_analytics;
			}
			set => s_analytics = value;
		}

		public static void Track(string eventName, Dictionary<string, string> properties)
		{
			Default.Track(eventName, properties);
		}

		public static void ReportException(Exception e)
		{
			Default.ReportException(e);
		}
	}

	class NullAnalytics : IAnalytics
	{
		public void Track(string eventName, Dictionary<string, string> properties)
		{
		}

		public void ReportException(Exception e)
		{
		}
	}
}
