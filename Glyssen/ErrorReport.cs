using System;

namespace Glyssen
{
	internal class ErrorReport
	{
		public static void ReportNonFatalMessageWithStackTrace(string getString, string getFileName, string languageFolder)
		{
			throw new System.NotImplementedException();
		}

		public static void ReportNonFatalExceptionWithMessage(Exception exception, string getString, string projectFilePath)
		{
			throw new NotImplementedException();
		}

		public static void NotifyUserOfProblem(Exception p0, string format)
		{
			throw new NotImplementedException();
		}

		internal static void NotifyUserOfProblem(string v1, string descriptionOfProjectBeingSplit, string descriptionOfProjectUsedToDetermineSplitLocations, string id, int chapterNumber, int verseNum, string v2)
		{
			throw new NotImplementedException();
		}

		internal static void ReportNonFatalExceptionWithMessage(Exception item1, string v)
		{
			throw new NotImplementedException();
		}

		public class NoNonFatalErrorReportExpected : IDisposable
		{
			public void Dispose()
			{
			}
		}

		public class NonFatalErrorReportExpected : IDisposable
		{
			public void Dispose()
			{
			}
		}
	}
}