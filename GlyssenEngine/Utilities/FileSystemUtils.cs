using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using SIL.IO;

namespace GlyssenEngine.Utilities
{
	public static class FileSystemUtils
	{
		// Note: Although technically a leading period is allowed, for our purposes in Glyssen,
		// that's unlikely to make sense.
		public static char[] TrimCharacters = { '.', ' ' };

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

		// Copied from Bloom's BookStorage and modified for efficiency (?) and to
		// prevent duplicate contiguous characters caused by replacements. Also
		// ensures that resulting filename is not a Windows reserved name.
		// This really belongs in libpalaso's PathUtilities (possibly with some additional options).
		// According to this website:
		// https://docs.microsoft.com/en-us/windows/desktop/fileio/naming-a-file
		// "When using an API to create a directory, the specified path cannot be so long that you cannot append
		// an 8.3 file name (that is, the directory name cannot exceed MAX_PATH minus 12)." MAX_PATH = 260,
		// but there is apparently no publicly accessible constant for either of these magic numbers in C# .Net.
		public static string RemoveDangerousCharacters(string name, int maxLength = 248, char replaceWith = ' ')
		{
			var dangerousCharacters = PathUtilities.GetInvalidOSIndependentFileNameChars();
			StringBuilder sb = new StringBuilder(name);

			for (var i = 0; i < sb.Length; i++)
			{
				char c = sb[i];
				// NBSP also causes problems.  See https://issues.bloomlibrary.org/youtrack/issue/BL-5212.
				if (dangerousCharacters.Contains(c) || c == '\u00a0')
				{
					if ((i > 0 && sb[i - 1] == replaceWith) || (i + 1 < sb.Length && sb[i + 1] == replaceWith))
						sb.Remove(i--, 1);
					else
						sb[i] = replaceWith;
				}
			}

			while (sb.Length > 1 && TrimCharacters.Contains(sb[0]))
				sb.Remove(0, 1);

			var result = sb.ToString().TrimEnd(TrimCharacters);
			if (IsReservedFilename(result))
				return String.Empty;
			return result.Length > maxLength ? result.Substring(0, maxLength) : result;
		}

		public static IReadOnlyList<string> GetIllegalFilenameCharacters(string filename)
		{
			var illegalCharacterNames = new HashSet<string>();
			foreach (var c in filename)
			{
				switch (c)
				{
					case '\u00a0': illegalCharacterNames.Add("NO-BREAK SPACE (U+00A0)"); break;
					case (char)0: illegalCharacterNames.Add("NULL (U+0000)"); break;
					case '\n':illegalCharacterNames.Add("LINE FEED (LF) (U+000A)"); break;
					case ':':
					case '<':
					case '>':
					case '\"':
					case '|':
					case '*':
					case '?':
					case '\\':
					case '/':
						illegalCharacterNames.Add(c.ToString());
						break;
					case '\u0001': illegalCharacterNames.Add("START OF HEADING (U+0001)"); break;
					case '\u0002': illegalCharacterNames.Add("START OF TEXT (U+0002)"); break;
					case '\u0003': illegalCharacterNames.Add("END OF TEXT (U+0003)"); break;
					case '\u0004': illegalCharacterNames.Add("END OF TRANSMISSION (U+0004)"); break;
					case '\u0005': illegalCharacterNames.Add("ENQUIRY (U+0005)"); break;
					case '\u0006': illegalCharacterNames.Add("ACKNOWLEDGE (U+0006)"); break;
					case '\a': illegalCharacterNames.Add("NO-BREAK SPACE (U+00A0)"); break;
					case '\b': illegalCharacterNames.Add("BACKSPACE (U+0008)"); break;
					case '\t': illegalCharacterNames.Add("CHARACTER TABULATION (U+0009)"); break;
					case '\v': illegalCharacterNames.Add("LINE TABULATION (U+000B)"); break;
					case '\f': illegalCharacterNames.Add("FORM FEED (FF) (U+000C)"); break;
					case '\r': illegalCharacterNames.Add("CARRIAGE RETURN (CR) (U+000D)"); break;
					case '\u000e': illegalCharacterNames.Add("SHIFT OUT (U+000E)"); break;
					case '\u000f': illegalCharacterNames.Add("SHIFT IN (U+000F)"); break;
					case '\u0010': illegalCharacterNames.Add("DATA LINK ESCAPE (U+0010)"); break;
					case '\u0011': illegalCharacterNames.Add("DEVICE CONTROL ONE (U+0011)"); break;
					case '\u0012': illegalCharacterNames.Add("DEVICE CONTROL TWO (U+0012)"); break;
					case '\u0013': illegalCharacterNames.Add("DEVICE CONTROL THREE (U+0013)"); break;
					case '\u0014': illegalCharacterNames.Add("DEVICE CONTROL FOUR (U+0014)"); break;
					case '\u0015': illegalCharacterNames.Add("NEGATIVE ACKNOWLEDGE (U+0015)"); break;
					case '\u0016': illegalCharacterNames.Add("SYNCHRONOUS IDLE (U+0016)"); break;
					case '\u0017': illegalCharacterNames.Add("END OF TRANSMISSION BLOCK (U+0017)"); break;
					case '\u0018': illegalCharacterNames.Add("CANCEL (U+0018)"); break;
					case '\u0019': illegalCharacterNames.Add("END OF MEDIUM (U+0019)"); break;
					case '\u001a': illegalCharacterNames.Add("SUBSTITUTE (U+001A)"); break;
					case '\u001b': illegalCharacterNames.Add("ESCAPE (U+001B)"); break;
					case '\u001c': illegalCharacterNames.Add("INFORMATION SEPARATOR FOUR (U+001C)"); break;
					case '\u001d': illegalCharacterNames.Add("INFORMATION SEPARATOR THREE (U+001D)"); break;
					case '\u001e': illegalCharacterNames.Add("INFORMATION SEPARATOR TWO (U+001E)"); break;
					case '\u001f': illegalCharacterNames.Add("INFORMATION SEPARATOR ONE (U+001F)"); break;
				}
			}
			return illegalCharacterNames.ToList();
		}

		public static bool StartsOrEndsWithDisallowedCharacters(string s)
		{
			return !String.IsNullOrEmpty(s) && (TrimCharacters.Any(c => c == s[0]) || TrimCharacters.Any(c => c == s.Last()));
		}

		// Checks the proposed filename to ensure it is not one of the Windows reserved file names,
		// enumerated here:
		// https://docs.microsoft.com/en-us/windows/desktop/fileio/naming-a-file
		public static bool IsReservedFilename(string proposedName)
		{
			try
			{
				proposedName = Path.GetFileNameWithoutExtension(proposedName);
			}
			catch (ArgumentException)
			{
				return false; // Illegal characters or null
			}
			switch (proposedName)
			{
				case "CON":
				case "PRN":
				case "AUX":
				case "NUL":
				case "COM1":
				case "COM2":
				case "COM3":
				case "COM4":
				case "COM5":
				case "COM6":
				case "COM7":
				case "COM8":
				case "COM9":
				case "LPT1":
				case "LPT2":
				case "LPT3":
				case "LPT4":
				case "LPT5":
				case "LPT6":
				case "LPT7":
				case "LPT8":
				case "LPT9":
					return true;
				default:
					return false;
			}
		}
	}
}
