using System;
using System.Linq;
using Glyssen.Shared;
using L10NSharp;

namespace Glyssen.Paratext
{
	internal class NoSupportedBooksException : ApplicationException
	{
		public object Details { get; }

		public NoSupportedBooksException(string projectName, ParatextProjectBookInfo details) : base(
			String.Format(LocalizationManager.GetString("Project.NoSupportedBooksInParatextProject",
				"{0} project {1} has no books which are supported by {2}",
				"Param 0: \"Paratext\" (product name); " +
				"Param 1: Project short name (unique project identifier); " +
				"Param 2: \"Glyssen\" (product name)"),
			ParatextScrTextWrapper.kParatextProgramName, projectName, GlyssenInfo.kProduct))
		{
			Details = details.Exclusions.Any() ? (object) details : $"No books in project {projectName}";
		}
	}
}
