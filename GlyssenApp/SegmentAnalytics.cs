using System;
using System.Collections.Generic;
using Glyssen;

namespace GlyssenApp
{
	class SegmentAnalytics : IAnalytics
	{
		public void Track(string eventName, Dictionary<string, string> properties)
		{
			DesktopAnalytics.Analytics.Track(eventName, properties);
		}

		public void ReportException(Exception e)
		{
			DesktopAnalytics.Analytics.ReportException(e);
		}
	}
}
