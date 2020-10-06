using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GlyssenEngine;
using GlyssenEngine.Bundle;
using GlyssenEngine.Rules;
using GlyssenEngine.ViewModels;
using L10NSharp;
using SIL.Progress;

namespace Glyssen.Dialogs
{
	class GenerateGroupsProgressDialog : ProgressDialogWithAcknowledgement
	{
		private readonly CharacterGroupGenerator m_generator;
		private readonly string m_sizeInfo;

		public GenerateGroupsProgressDialog(Project project, CharacterGroupGenerator generator, bool firstRun, bool replaceCancelButtonWithLink = false)
		{
			m_generator = generator;
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
			CancelLinkText = LocalizationManager.GetString("DialogBoxes.GenerateGroupsProgressDialog.CancelLinkText",
				"No! Let me group character roles manually.");
			ProgressLabelTextWhenComplete = LocalizationManager.GetString("DialogBoxes.GenerateGroupsProgressDialog.Complete",
				"Group generation is complete.");
			BarStyle = ProgressBarStyle.Marquee;
		}

		protected override void DoWork(CancellationToken cancellationToken)
		{
			try
			{
				m_generator.GenerateCharacterGroups(cancellationToken);
			}
			catch
			{
				if (!CanCancel)
					throw;
				var msg = LocalizationManager.GetString("DialogBoxes.GenerateGroupsProgressDialog.GenerationFailed",
					"New character groups could not be generated to satisfy the project settings for the current cast size. ({0})",
					"Parameter is a statement about the number of voice actors or planned cast size.");
					MessageBox.Show(this, String.Format(msg, m_sizeInfo), Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				DialogResult = DialogResult.Cancel;
				Close();
			}
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

		public static void GenerateGroupsWithProgress(Project project, bool attemptToPreserveActorAssignments, bool firstGroupGenerationRun, bool forceMatchToActors, CastSizeRowValues ghostCastSize = null, bool cancelLink = false)
		{
			var castSizeOption = project.CharacterGroupGenerationPreferences.CastSizeOption;
			if (forceMatchToActors)
				project.CharacterGroupGenerationPreferences.CastSizeOption = CastSizeOption.MatchVoiceActorList;
			bool saveGroups = false;

			var generator = new CharacterGroupGenerator(project, ghostCastSize);
			using (var progressDialog = new GenerateGroupsProgressDialog(project, generator, firstGroupGenerationRun, cancelLink))
			{
				if (progressDialog.ShowDialog() == DialogResult.OK && generator.GeneratedGroups != null)
				{
					var assignedBefore = project.CharacterGroupList.CountVoiceActorsAssigned();
					generator.ApplyGeneratedGroupsToProject(attemptToPreserveActorAssignments);

					if (project.CharacterGroupList.CountVoiceActorsAssigned() < assignedBefore)
					{
						var msg = LocalizationManager.GetString("MainForm.FewerAssignedActorsAfterGeneration",
							"An actor assignment had to be removed. Please review the Voice Actor assignments, and adjust where necessary.");
						MessageBox.Show(msg);
					}

					saveGroups = true;
				}
				else if (forceMatchToActors)
					project.CharacterGroupGenerationPreferences.CastSizeOption = castSizeOption;
			}
			project.Save(saveGroups);
		}
	}
}
