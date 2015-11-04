using System;
using System.ComponentModel;
using System.Windows.Forms;
using L10NSharp;

namespace Glyssen.Dialogs
{
	class GenerateGroupsProgressDialog : ProgressDialogWithAcknowledgement
	{
		public GenerateGroupsProgressDialog(Project project, DoWorkEventHandler doWorkEventHandler)
		{
			int numCharacters = project.GetKeyStrokesByCharacterId().Count;
			int numActors = project.VoiceActorList.Actors.Count;
			Overview =
				string.Format(LocalizationManager.GetString("DialogBoxes.GenerateGroupsProgressDialog.Overview.Text1",
					"This project has {0} characters and {1} voice actors."),
					numCharacters, numActors) +
				Environment.NewLine +
				string.Format(LocalizationManager.GetString("DialogBoxes.GenerateGroupsProgressDialog.Overview.Text2",
					"{0} is attempting to select the best configuration of character groups out of many possible configurations.  " +
					"Characters are placed together based on their attributes and separated into various groups to optimize " +
					"the space between characters spoken by the same actor."),
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
