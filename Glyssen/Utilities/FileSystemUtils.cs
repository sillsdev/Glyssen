using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using SIL.IO;

namespace Glyssen.Utilities
{
	public static class FileSystemUtils
	{
		public static void SafeCreateAndOpenFolder(string folderPath)
		{
			try
			{
				Directory.CreateDirectory(folderPath);
				Process.Start(folderPath);
			}
			catch (Exception)
			{
				// Ignore;
			}
		}

		// Copied from Bloom's BookStorage and modified for efficiency (like it really matters).
		// This really belongs in libpalaso's PathUtilities.
		public static string RemoveDangerousCharacters(string name, char replaceWith = ' ')
		{
			var dangerousCharacters = PathUtilities.GetInvalidOSIndependentFileNameChars();
			var trimCharacters = new [] { '.', ' ', '\t' };
			StringBuilder sb = new StringBuilder(name);

			for (var i = name.Length - 1; i >= 0; i--)
			{
				char c = name[i];
				// NBSP also causes problems.  See https://issues.bloomlibrary.org/youtrack/issue/BL-5212.
				if (dangerousCharacters.Contains(c) || c == '\u00a0')
					sb[i] = replaceWith;
			}

			while (sb.Length > 1 && trimCharacters.Contains(sb[0]))
				sb.Remove(0, 1);

			return sb.ToString().TrimEnd(trimCharacters);
		}
	}
}
