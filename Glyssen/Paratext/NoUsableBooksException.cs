using System;
using L10NSharp;

namespace Glyssen.Paratext
{
	internal class NoUsableBooksException : ApplicationException
	{
		public DisallowedBookInfo Details { get; private set; }

		public NoUsableBooksException(string projectName, DisallowedBookInfo details) : base(
			String.Format(LocalizationManager.GetString("Project.NoUsableBooksInParatextProject",
				"No usable books were found in {0} project: {1}",
				"Param 0: \"Paratext\" (product name); Param 1: Project short name (unique project identifier)"),
			ParatextScrTextWrapper.kParatextProgramName, projectName))
		{
			Details = details;
		}
	}
}
