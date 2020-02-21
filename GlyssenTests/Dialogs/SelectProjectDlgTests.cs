using System.Threading;
using Glyssen.Dialogs;
using NUnit.Framework;

namespace GlyssenTests.Dialogs
{
	[TestFixture]
	[Apartment(ApartmentState.STA)]
	public class SelectProjectDlgTests
	{
		[Test, Explicit] // by hand only
		public void ShowDialog()
		{
			using (var dlg = new SelectProjectDlg())
			{
				dlg.ShowDialog();
			}
		}
	}
}
