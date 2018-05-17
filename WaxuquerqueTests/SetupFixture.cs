using System;
using Glyssen.Utilities;
using NUnit.Framework;
using Waxuquerque.Utilities;

namespace WaxuquerqueTests
{
	/// <summary>
	/// The methods in this class run once before and after each test run, i.e. they get executed exactly once.
	/// </summary>
	[SetUpFixture]
	public class SetupFixture
	{
		[SetUp]
		public void Setup()
		{
			Logger.Default = new NullLogger();
			UserSettings.Default = new NullUserSettings();
			ErrorReport.Default = new DesktopErrorReport();
		}

		[TearDown]
		public void TearDown()
		{
		}
	}

	public class NullLogger : ILogger
	{
		public void WriteEvent(string message, params object[] args)
		{
		}

		public void WriteMinorEvent(string message, params object[] args)
		{
		}

		public void WriteError(Exception e)
		{
		}

		public void WriteError(string msg, Exception e)
		{
		}
	}

	public class NullUserSettings : IUserSettings
	{
		public string CurrentProject { get; set; }
		public string DefaultExportDirectory { get; set; }
	}
}
