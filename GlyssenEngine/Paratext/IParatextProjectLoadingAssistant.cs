using System;
using System.Collections.Generic;

namespace GlyssenEngine.Paratext
{
	public interface IParatextProjectLoadingAssistant
	{
		Project Project { get; set; }
		bool SilentMode { get; set; }
		string ParatextProjectName { get; set; }
		bool RetryWhenProjectNotFound();
		bool ForceReload { get; }
		bool RetryWhenReloadFails(string error);
		void ReportApplicationError(ApplicationException exception);
		bool ConfirmUpdateThatWouldExcludeExistingBooks(IReadOnlyCollection<string> noLongerAvailableBookIds, IReadOnlyCollection<string> noLongerPassingListBookIds);
		bool ConfirmUpdateGlyssenProjectMetadataIdToMatchParatextProject(string msg);
		void HandleProjectPathChanged();
	}
}
