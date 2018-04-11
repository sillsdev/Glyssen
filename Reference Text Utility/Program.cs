using System;
using System.Windows.Forms;
using SIL.Reporting;
using SIL.Windows.Forms.Reporting;

namespace Glyssen.ReferenceTextUtility
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			SetUpErrorHandling();
			Application.Run(new RefTextUtilityForm());
		}

		private static void SetUpErrorHandling()
		{
			SIL.Reporting.ErrorReport.SetErrorReporter(new WinFormsErrorReporter());
			SIL.Reporting.ErrorReport.EmailAddress = "glyssen-support_lsdev@sil.org";
			SIL.Reporting.ErrorReport.AddStandardProperties();
			ExceptionHandler.Init(new WinFormsExceptionHandler());
		}
	}
}
