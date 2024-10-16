﻿using System;
using Glyssen.Shared;
using SIL;

namespace GlyssenEngine.Paratext
{
	internal class NoSupportedBooksException : ApplicationException
	{
		public ParatextProjectBookInfo Details { get; }

		public NoSupportedBooksException(string projectName, ParatextProjectBookInfo details) : base(
			String.Format(Localizer.GetString("Project.NoSupportedBooksInParatextProject",
				"{0} project {1} has no books which are supported by {2}.",
				"Param 0: \"Paratext\" (product name); " +
				"Param 1: Paratext project short name (unique project identifier); " +
				"Param 2: Product name (e.g., \"Glyssen\")"),
			ParatextScrTextWrapper.kParatextProgramName, projectName, GlyssenInfo.Product))
		{
			Details = details;
		}
	}
}
