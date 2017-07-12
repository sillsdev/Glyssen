using System;
using System.IO;
using System.Linq;
using SIL.Scripture;

namespace Glyssen.Shared
{
	/// <summary>
	/// Shared utilities for processing projects
	/// </summary>
	public static class ProjectUtilities
	{
		public static void ForEachBookFileInProject(string projectDir, Action<string, string> action)
		{
			string[] files = Directory.GetFiles(projectDir, "???" + Constants.kBookScriptFileExtension);
			for (int i = 1; i <= BCVRef.LastBook; i++)
			{
				string bookCode = BCVRef.NumberToBookCode(i);
				string possibleFileName = Path.Combine(projectDir, bookCode + Constants.kBookScriptFileExtension);
				if (files.Contains(possibleFileName))
					action(bookCode, possibleFileName);
			}
		}
	}
}