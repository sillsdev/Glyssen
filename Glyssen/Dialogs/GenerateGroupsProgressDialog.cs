using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Glyssen.Bundle;
using L10NSharp;

namespace Glyssen.Dialogs
{
	class GenerateGroupsProgressDialog : ProgressDialogWithAcknowledgement
	{
		public GenerateGroupsProgressDialog(Project project, DoWorkEventHandler doWorkEventHandler, bool firstRun, bool replaceCancelButtonWithLink = false)
		{
			InitializeComponent();

			Text = LocalizationManager.GetString("DialogBoxes.GenerateGroupsProgressDialog.Title", "Generating Groups");

			CharacterGroupGenerationPreferences groupGenerationPreferences = project.CharacterGroupGenerationPreferences;
			int numCharacters = project.TotalCharacterCount;
			int numActors = groupGenerationPreferences.CastSizeOption == CastSizeOption.MatchVoiceActorList ?
				project.VoiceActorList.ActiveActors.Count() :
				new CastSizePlanningViewModel(project).GetCastSizeRowValues(groupGenerationPreferences.CastSizeOption).Total;
			string firstLineOfText = string.Format(LocalizationManager.GetString("DialogBoxes.GenerateGroupsProgressDialog.Overview.Text1",
				"The recording project has {0} distinct Biblical character roles."), numCharacters);
			string secondLineOfText = groupGenerationPreferences.CastSizeOption == CastSizeOption.MatchVoiceActorList ?
				string.Format(LocalizationManager.GetString("DialogBoxes.GenerateGroupsProgressDialog.Overview.Text2.MatchActors",
				"The voice actor list has {0} voice actors."), numActors) :
				string.Format(LocalizationManager.GetString("DialogBoxes.GenerateGroupsProgressDialog.Overview.Text2.CastSize",
				"The planned cast size is {0} voice actors."), numActors);
			string firstLineOfStatusText = groupGenerationPreferences.CastSizeOption == CastSizeOption.MatchVoiceActorList ?
				string.Format(LocalizationManager.GetString("DialogBoxes.GenerateGroupsProgressDialog.StatusText.Text1.MatchActors",
					"{0} is creating optimized groups of character roles to match the voice actor list."), ProductName) :
				string.Format(LocalizationManager.GetString("DialogBoxes.GenerateGroupsProgressDialog.StatusText.Text1.CastSize",
					"{0} is creating optimized groups of character roles to match the cast size."), ProductName);

			Overview = firstLineOfText + Environment.NewLine + secondLineOfText;
			if (firstRun)
				StatusText = firstLineOfStatusText;
			else
				StatusText =
					firstLineOfStatusText +
					Environment.NewLine +
					string.Format(LocalizationManager.GetString("DialogBoxes.GenerateGroupsProgressDialog.StatusText.Text2.SubsequentRun",
						"{0} will attempt to maintain any existing voice actor assignments."),
						ProductName);

			OkButtonText = firstRun ? LocalizationManager.GetString("DialogBoxes.GenerateGroupsProgressDialog.Continue", "Continue") :
				LocalizationManager.GetString("Common.OK", "OK");
			ShowInTaskbar = false;
			CanCancel = !firstRun;
			ReplaceCancelButtonWithLink = replaceCancelButtonWithLink;
			ProgressLabelTextWhenComplete = LocalizationManager.GetString("DialogBoxes.GenerateGroupsProgressDialog.Complete", "Group generation is complete.");
			BarStyle = ProgressBarStyle.Marquee;
			BackgroundWorker worker = new BackgroundWorker();
			worker.WorkerSupportsCancellation = true;
			worker.DoWork += doWorkEventHandler;
			worker.RunWorkerCompleted += (s, e) => { if (e.Error != null) throw e.Error; };
			BackgroundWorker = worker;
		}

		private void InitializeComponent()
		{
			SuspendLayout();
			AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			ClientSize = new System.Drawing.Size(465, 132);
			Location = new System.Drawing.Point(0, 0);
			Name = "GenerateGroupsProgressDialog";
			ResumeLayout(false);
			PerformLayout();

		}
	}
}
