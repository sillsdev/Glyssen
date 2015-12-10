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
			Text = LocalizationManager.GetString("DialogBoxes.GenerateGroupsProgressDialog.Title", "Optimize Groups");

			int numCharacters = project.GetKeyStrokesByCharacterId().Count;
			int numActors = project.VoiceActorList.Actors.Count;
			string secondLineOfText = string.Format(LocalizationManager.GetString("DialogBoxes.GenerateGroupsProgressDialog.Overview.AnyRun.Text2",
				"{0} is optimizing groups of character roles to match the list of actors."),
				ProductName);
			if (firstRun)
				Overview =
					string.Format(LocalizationManager.GetString("DialogBoxes.GenerateGroupsProgressDialog.Overview.FirstRun.Text1",
						"This project has {0} distinct character roles and {1} voice actors."),
						numCharacters, numActors) +
					Environment.NewLine +
					secondLineOfText;
			else
				Overview =
					string.Format(LocalizationManager.GetString("DialogBoxes.GenerateGroupsProgressDialog.Overview.SubsequentRun.Text1",
						"This project now has {0} distinct character roles and {1} voice actors."),
						numCharacters, numActors) +
					Environment.NewLine +
					secondLineOfText +
					Environment.NewLine +
					string.Format(LocalizationManager.GetString("DialogBoxes.GenerateGroupsProgressDialog.Overview.SubsequentRun.Text3",
						"{0} will attempt to maintain any existing voice actor assignments."),
						ProductName);

			ShowInTaskbar = false;
			CanCancel = false;
			ProgressLabelTextWhenComplete = LocalizationManager.GetString("GenerateGroupsProgressDialog.Complete", "Group generation is complete");
			BarStyle = ProgressBarStyle.Marquee;
			BackgroundWorker worker = new BackgroundWorker();
			worker.DoWork += doWorkEventHandler;
			worker.RunWorkerCompleted += (s, e) => { if (e.Error != null) throw e.Error; };
			BackgroundWorker = worker;
		}
	}
}
