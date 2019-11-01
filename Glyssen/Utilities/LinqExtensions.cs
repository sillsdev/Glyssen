using System;
using System.Collections.Generic;
using System.Linq;

namespace Glyssen.Utilities
{
	public static class LinqExtensions
	{
		// The following methods are like SingleOrDefault, but don't throw an exception if there is more than one;
		// and like FirstOrDefault, except they return the default if there is more than one.

		/// <summary>
		/// Returns the only element of a sequence that satisfies the specified condition, or a default value if there is not
		/// exactly one such element.
		/// </summary>
		public static T OnlyOrDefault<T>(this IEnumerable<T> source, Func<T, bool> predicate) => OnlyOrDefault(source.Where(predicate));

		/// <summary>
		/// Returns the only element of a sequence or a default value if there is not exactly one element in the sequence.
		/// </summary>
		public static T OnlyOrDefault<T>(this IEnumerable<T> source)
		{
			bool any = false;
			T first = default(T);
			foreach (var t in source)
			{
				if (any)
					return default(T);
				first = t;
				any = true;
			}

			return first;
		}
	}
}
