using System;
using System.IO;
using System.Linq;
using System.Text;
using SIL.Unicode;

namespace GlyssenEngine.Utilities
{
	public static class StringExtensions
	{
		public static bool Contains(this string source, string toCheck, StringComparison comp)
		{
			return source.IndexOf(toCheck, comp) >= 0;
		}

		public static string ReplaceFirst(this string text, string search, string replace)
		{
			int pos = text.IndexOf(search);
			if (pos < 0)
			{
				return text;
			}
			return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
		}

		public static void AppendParagraph(this StringBuilder sb, string paragraphText)
		{
			sb.AppendLine(string.Format("<p>{0}</p>", paragraphText));
		}

		public static bool IsWhitespace(this string value)
		{
			if (string.IsNullOrEmpty(value))
				return false;

			// ReSharper disable once SimplifyConditionalTernaryExpression
			return value.Any(t => !char.IsWhiteSpace(t)) ? false : true;
		}

		public static bool EndsWithSentenceEndingPunctuation(this string text)
		{
			int i = text.Length - 1;
			while (i >= 0)
			{
				char c = text[i];
				if (char.IsPunctuation(c))
				{
					if (CharacterUtils.IsSentenceFinalPunctuation(c))
					{
						return true;
					}
				}
				else if (!char.IsWhiteSpace(c) && !char.IsSymbol(c))
				{
					return false;
				}
				i--;
			}
			return false;
		}

		public static string GetContainingFolderName(this string text)
		{
			return Path.GetFileName(Path.GetDirectoryName(text));
		}

		public static string Truncate(this string text, int to, string ellipses = "\u2026")
		{
			if (text == null)
				throw new ArgumentNullException(nameof(text));
			if (ellipses == null)
				throw new ArgumentNullException(nameof(ellipses));
			if (to <= ellipses.Length)
				throw new ArgumentOutOfRangeException(nameof(to), $"String cannot be truncated to a length less than or equal to the length of {nameof(ellipses)} ({ellipses.Length}).");

			if (text.Length <= to)
				return text;
			var truncated = text.TrimEnd();
			if (truncated.Length <= to)
				return truncated;
			if (to + ellipses.Length >= truncated.Length)
				return truncated;
			return truncated.Substring(0, to).TrimEnd() + ellipses;
		}
	}
}
