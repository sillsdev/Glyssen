using System;
using System.Text;

namespace Glyssen.Utilities
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
	}
}
