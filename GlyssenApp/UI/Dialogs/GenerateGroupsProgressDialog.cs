using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using L10NSharp;
using Waxuquerque;
using Waxuquerque.Bundle;
using Waxuquerque.ViewModel;

namespace GlyssenApp.UI.Dialogs
{
	public class GenerateGroupsProgressDialog : ProgressDialogWithAcknowledgement
	{
		private readonly string m_sizeInfo;

		public GenerateGroupsProgressDialog(Project project, BackgroundWorker backgroundWorker, bool firstRun, bool replaceCancelButtonWithLink = false)
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
			m_sizeInfo = groupGenerationPreferences.CastSizeOption == CastSizeOption.MatchVoiceActorList ?
				string.Format(LocalizationManager.GetString("DialogBoxes.GenerateGroupsProgressDialog.Overview.Text2.MatchActors",
				"The voice actor list has {0} voice actors."), numActors) :
				string.Format(LocalizationManager.GetString("DialogBoxes.GenerateGroupsProgressDialog.Overview.Text2.CastSize",
				"The planned cast size is {0} voice actors."), numActors);
			string firstLineOfStatusText = groupGenerationPreferences.CastSizeOption == CastSizeOption.MatchVoiceActorList ?
				string.Format(LocalizationManager.GetString("DialogBoxes.GenerateGroupsProgressDialog.StatusText.Text1.MatchActors",
					"{0} is creating optimized groups of character roles to match the voice actor list."), ProductName) :
				string.Format(LocalizationManager.GetString("DialogBoxes.GenerateGroupsProgressDialog.StatusText.Text1.CastSize",
					"{0} is creating optimized groups of character roles to match the cast size."), ProductName);

			Overview = firstLineOfText + Environment.NewLine + m_sizeInfo;
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
			BackgroundWorker = backgroundWorker;
		}

		protected override void OnBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Error != null)
			{
				if (!CanCancel)
					throw e.Error;
				var msg = LocalizationManager.GetString("DialogBoxes.GenerateGroupsProgressDialog.GenerationFailed",
					"New character groups could not be generated to satisfy the project settings for the current cast size. ({0})",
					"Parameter is a statement about the number of voice actors or planned cast size.");
					MessageBox.Show(this, String.Format(msg, m_sizeInfo), Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				DialogResult = DialogResult.Cancel;
				Close();
			}
			else
				base.OnBackgroundWorker_RunWorkerCompleted(sender, e);
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
