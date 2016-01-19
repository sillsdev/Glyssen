using System;
using System.ComponentModel;
using System.Windows.Forms;
using L10NSharp;

namespace Glyssen.Dialogs
{
	class GenerateGroupsProgressDialog : ProgressDialogWithAcknowledgement
	{
		public GenerateGroupsProgressDialog(Project project, DoWorkEventHandler doWorkEventHandler, bool firstRun)
		{
			InitializeComponent();

			Text = LocalizationManager.GetString("DialogBoxes.GenerateGroupsProgressDialog.Title", "Optimize Groups");

			int numCharacters = project.GetKeyStrokesByCharacterId().Count;
			int numActors = project.VoiceActorList.Actors.Count;
			string firstLineOfText = string.Format(LocalizationManager.GetString("DialogBoxes.GenerateGroupsProgressDialog.Overview.AnyRun.Text1",
				"Script includes {0} distinct Biblical character roles."), numCharacters);
			string secondLineOfText = string.Format(LocalizationManager.GetString("DialogBoxes.GenerateGroupsProgressDialog.Overview.AnyRun.Text2",
				"You have entered {0} voice actors."), numActors);
			string firstLineOfStatusText = string.Format(LocalizationManager.GetString("DialogBoxes.GenerateGroupsProgressDialog.StatusText.AnyRun.Text1",
				"{0} is creating optimized groups of characters to match the list of actors."),
				ProductName);

			Overview = firstLineOfText + Environment.NewLine + secondLineOfText;
			if (firstRun)
				StatusText = firstLineOfStatusText;
			else
				StatusText =
					firstLineOfStatusText +
					Environment.NewLine +
					string.Format(LocalizationManager.GetString("DialogBoxes.GenerateGroupsProgressDialog.StatusText.SubsequentRun.Text2",
						"{0} will attempt to maintain any existing voice actor assignments."),
						ProductName);

			OkButtonText = LocalizationManager.GetString("DialogBoxes.GenerateGroupsProgressDialog.Continue", "Continue");
			ShowInTaskbar = false;
			CanCancel = false;
			ProgressLabelTextWhenComplete = LocalizationManager.GetString("GenerateGroupsProgressDialog.Complete", "Group generation is complete.");
			BarStyle = ProgressBarStyle.Marquee;
			BackgroundWorker worker = new BackgroundWorker();
			worker.WorkerSupportsCancellation = true;
			worker.DoWork += doWorkEventHandler;
			worker.RunWorkerCompleted += (s, e) => { if (e.Error != null) throw e.Error; };
			BackgroundWorker = worker;
		}

		private void InitializeComponent()
		{
			this.SuspendLayout();
			//
			// GenerateGroupsProgressDialog
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.ClientSize = new System.Drawing.Size(465, 132);
			this.Location = new System.Drawing.Point(0, 0);
			this.Name = "GenerateGroupsProgressDialog";
			this.ResumeLayout(false);
			this.PerformLayout();

		}
	}
}
