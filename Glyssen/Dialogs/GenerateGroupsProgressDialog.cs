using System.ComponentModel;
using System.Windows.Forms;
using L10NSharp;
using SIL.Windows.Forms.Progress;

namespace Glyssen.Dialogs
{
	class GenerateGroupsProgressDialog : ProgressDialog
	{
		public GenerateGroupsProgressDialog(DoWorkEventHandler doWorkEventHandler)
		{
			ShowInTaskbar = false;
			Overview = LocalizationManager.GetString("DialogBoxes.GenerateGroupsProgressDialog.Overview", "Generating optimal character groups based on voice actor attributes.");
			CanCancel = false;
			BarStyle = ProgressBarStyle.Marquee;
			BackgroundWorker worker = new BackgroundWorker();
			worker.DoWork += doWorkEventHandler;
			worker.RunWorkerCompleted += (s, e) => { if (e.Error != null) throw e.Error; };
			BackgroundWorker = worker;
		}
	}
}
