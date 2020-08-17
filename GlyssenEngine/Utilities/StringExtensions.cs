using System;
using System.IO;
using System.Linq;
using System.Text;
using SIL.Unicode;

namespace GlyssenEngine.Utilities
{
	public static class StringExtensions
	{
		/// <summary>
		/// Deprecated: use GetOptionalAttributeValue instead.
		/// </summary>
		// ENHANCE: Sometime when we're making a breaking change, this can be removed.
		[Obsolete("Use SIL.Extensions.StringExtensions.Contains instead")]
		public static bool Contains(this string source, string toCheck, StringComparison comp)
		{
			return SIL.Extensions.StringExtensions.Contains(source, toCheck, comp);
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

		/// <summary>
		/// Truncates a string such that the resulting length is guaranteed to be no longer than
		/// the specified length plus the length of the ellipses character(s). If the string ends
		/// in whitespace and it is longer than the specified, it will first be trimmed. If it is
		/// still longer than the specified length, it will be truncated. If truncated, it is
		/// guaranteed that the ellipses character(s) will never be preceded by whitespace. (Thus
		/// truncation can result in a string that is shorter than the specified length plus the
		/// length of the ellipses characters).
		/// </summary>
		/// <param name="text">The string to truncate</param>
		/// <param name="to">The position at which truncation is to occur if needed</param>
		/// <param name="ellipses">The ellipses character (default) or other string to indicate
		/// that the string was truncated. (The ellipses string will be applied as is, with no
		/// trimming, so if leading or trailing whitespace is desired, it can be included.</param>
		/// <returns>The resulting (possibly truncated and/or trimmed) text</returns>
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
			if (to + ellipses.Length >= truncated.Length)
				return truncated;
			return truncated.Substring(0, to).TrimEnd() + ellipses;
		}
	}
}
