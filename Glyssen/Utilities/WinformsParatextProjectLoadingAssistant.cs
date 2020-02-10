using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Glyssen.Shared;
using GlyssenEngine;
using GlyssenEngine.Paratext;
using L10NSharp;
using SIL.Reporting;
using static System.String;

namespace Glyssen.Utilities
{
	class WinformsParatextProjectLoadingAssistant : IParatextProjectLoadingAssistant
	{
		public string Context { get; }
		public bool ForceReload { get; }

		public Project Project { get; set; }
		public bool SilentMode { get; set; }
		public string ParatextProjectName { get; set; }

		public WinformsParatextProjectLoadingAssistant(string context, bool forceReload)
		{
			Context = context;
			ForceReload = forceReload;
		}

		public bool RetryWhenProjectNotFound()
		{
			if (SilentMode)
				return false;
			string msg = Context + Format(
					LocalizationManager.GetString("Project.ParatextProjectMissingMsg",
						"To update the {0} project, the {1} project {2} must be available, but it is not.",
						"Param 0: Glyssen recording project name; " +
						"Param 1: \"Paratext\" (product name); " +
						"Param 2: Paratext project short name (unique project identifier)"),
					Project.Name, ParatextScrTextWrapper.kParatextProgramName, ParatextProjectName) +
				Environment.NewLine + Environment.NewLine +
				Format(LocalizationManager.GetString("Project.RestoreParatextProject",
						"If possible, you can restore the {0} project and retry; otherwise, you can cancel and {1} will continue to work with the existing project data.",
						"Param 0: \"Paratext\" (product name); Param 1: \"Glyssen\" (product name)"),
					ParatextScrTextWrapper.kParatextProgramName, GlyssenInfo.kProduct);

			string caption = Format(LocalizationManager.GetString("Project.ParatextProjectUnavailable", "{0} Project Unavailable",
					"Param is \"Paratext\" (product name)"),
				ParatextScrTextWrapper.kParatextProgramName);
			return (DialogResult.Retry == MessageBox.Show(msg, caption, MessageBoxButtons.RetryCancel));
		}

		public bool RetryWhenReloadFails(string error)
		{
			if (SilentMode)
				return false;

			string msg = Context + Format(
				LocalizationManager.GetString("Project.ParatextProjectReloadFailure",
					"An error occurred reloading the {0} project {1}:\r\n{2}\r\n\r\n" +
					"If you cannot fix the problem, you can cancel and continue to work with the existing project data.",
					"Param 0: \"Paratext\" (product name); " +
					"Param 1: Paratext project short name (unique project identifier); " +
					"Param 2: Specific error message"),
				ParatextScrTextWrapper.kParatextProgramName, ParatextProjectName, error);
			return DialogResult.Retry == MessageBox.Show(msg, GlyssenInfo.kProduct, MessageBoxButtons.RetryCancel);
		}

		public bool ConfirmUpdateThatWouldExcludeExistingBooks(IReadOnlyCollection<string> noLongerAvailableBookIds, IReadOnlyCollection<string> noLongerPassingListBookIds)
		{
			string msg = Context + Format(LocalizationManager.GetString("Project.ParatextProjectUpdateExcludedBooksWarning",
				"{1} detected changes in the {2} project {3} that would result in the exclusion " +
				"of books from the {0} project that were previously included:",
				"Param 0: \"Glyssen\" (product name); " +
				"Param 1: \"Paratext\" (product name); " +
				"Param 2: Paratext project short name (unique project identifier); " +
				"Param 3: Glyssen recording project name"),
			GlyssenInfo.kProduct,
			ParatextScrTextWrapper.kParatextProgramName,
			ParatextProjectName,
			Project.Name);

			if (noLongerAvailableBookIds.Any())
			{
				msg += Environment.NewLine + LocalizationManager.GetString("Project.ParatextBooksNoLongerAvailable",
						"The following books are no longer available:") + Environment.NewLine + "   " +
					Join(LocalizationManager.GetString("Common.SimpleListSeparator", ", "), noLongerAvailableBookIds);
			}
			if (noLongerPassingListBookIds.Any())
			{
				var scriptureRangeSelectionDlgName = LocalizationManager.GetString(
					"DialogBoxes.ScriptureRangeSelectionDlg.WindowTitle", "Select Books - {0}", "{0} is the project name");
				Debug.Assert(LocalizationManager.UILanguageId != "en" || scriptureRangeSelectionDlgName == "Select Books - {0}",
					"Dev alert: this localized string and ID MUST be kept in sync with the version in ScriptureRangeSelectionDlg.Designer.cs!");
				scriptureRangeSelectionDlgName = Format(scriptureRangeSelectionDlgName, "");
				while (!Char.IsLetter(scriptureRangeSelectionDlgName.Last()))
					scriptureRangeSelectionDlgName = scriptureRangeSelectionDlgName.Remove(scriptureRangeSelectionDlgName.Length - 1);
				msg += Environment.NewLine + Format(LocalizationManager.GetString("Project.ParatextBooksNoLongerPassChecks",
						"The following books no longer appear to pass the basic checks:\r\n   {0}\r\n" +
						"(If needed, you can include these books again later in the {1} dialog box.)",
						"Param 0: list of 3-letter book IDs; " +
						"Param 1: Name of the \"Select Books\" dialog box."),
					Join(LocalizationManager.GetString("Common.SimpleListSeparator", ", "), noLongerPassingListBookIds),
					scriptureRangeSelectionDlgName);
			}

			msg += Environment.NewLine + Environment.NewLine +
				LocalizationManager.GetString("Project.ParatextProjectUpdateConfirmExcludeBooks",
					"Would you like to proceed with the update?");

			return DialogResult.Yes == MessageBox.Show(msg, GlyssenInfo.kProduct, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
		}

		public bool ConfirmUpdateGlyssenProjectMetadataIdToMatchParatextProject(string msg)
		{
			if (SilentMode)
				return false;

			var question = Format(LocalizationManager.GetString("Project.UpdateParatextProjectIdQuestion",
					"Do you want to continue, updating the {0} project to use the new {1} project ID?",
					"Param 0: \"Glyssen\" (product name); " +
					"Param 1: \"Paratext\" (product name)"),
				GlyssenInfo.kProduct,
				ParatextScrTextWrapper.kParatextProgramName);

			return MessageBox.Show(msg + Environment.NewLine + Environment.NewLine + question, GlyssenInfo.kProduct,
				MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes;
		}

		public void HandleProjectPathChanged()
		{
			Properties.Settings.Default.CurrentProject = Project.ProjectFilePath;
		}

		public void ReportApplicationError(ApplicationException exception)
		{
			if (SilentMode)
			{
				Logger.WriteError(exception);
				return;
			}

			string msg = Context + Format(LocalizationManager.GetString("Project.ParatextProjectUpdateErrorMsg",
						"To update the {0} project, {1} attempted to get the current text of the books from the {2} project {3}, but there was a problem:",
						"Param 0: Glyssen recording project name; " +
						"Param 1: \"Glyssen\" (product name); " +
						"Param 2: \"Paratext\" (product name); " +
						"Param 3: Paratext project short name (unique project identifier)"),
					Project.Name,
					GlyssenInfo.kProduct,
					ParatextScrTextWrapper.kParatextProgramName,
					ParatextProjectName) +
				Environment.NewLine + exception.Message;

			var inner = exception.InnerException;
			while (inner != null)
			{
				msg += Environment.NewLine + inner.Message;
				inner = inner.InnerException;
			}

			MessageBox.Show(msg, GlyssenInfo.kProduct, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
		}
	}
}
